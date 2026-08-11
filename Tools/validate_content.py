#!/usr/bin/env python3
"""License-free CI mirror of Assets/Editor/ContentValidator.cs (issue #7).

Cross-checks the three content JSON folders — AbilityData, CatalogData,
JobData — without needing a Unity editor or license, so pull requests get a
fast data gate on any runner. The C# validator remains the in-editor
authority (it also gates generation); keep the two rule sets in sync when
either changes.

Exit code 0 = valid (warnings allowed), 1 = at least one error.
"""

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ABILITY_DIR = ROOT / "Assets/Resources/AbilityData"
CATALOG_DIR = ROOT / "Assets/Resources/CatalogData"
JOB_DIR = ROOT / "Assets/Resources/JobData"

JP_THRESHOLD_COUNT = 7  # levels 2-8; must match JobDefinition.JPThresholdCount
MAX_JOB_LEVEL = 8

errors: list[str] = []
warnings: list[str] = []


def slug(name: str) -> str:
    """Mirrors the generators' fallback slug for data without stable ids."""
    if not name:
        return ""
    return re.sub(r"[^a-z0-9]+", "_", name.strip().lower()).strip("_")


def json_files(path: Path) -> list[Path]:
    """A missing or empty dataset folder is a hard error: generators delete
    existing output before writing, so an emptied folder would erase
    generated content while 'succeeding'."""
    if not path.is_dir():
        errors.append(f"content folder missing: {path.relative_to(ROOT)}")
        return []
    files = sorted(path.glob("*.json"))
    if not files:
        errors.append(f"content folder has no JSON files: {path.relative_to(ROOT)}")
    return files


def load(path: Path):
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except Exception as exc:  # noqa: BLE001 — every parse failure is one error
        errors.append(f"{path.name}: failed to parse — {exc}")
        return None


def main() -> int:
    ability_ids_by_job: dict[str, set[str]] = {}
    ability_names_by_job: dict[str, set[str]] = {}
    all_ability_ids: set[str] = set()
    unlocked_ability_ids: set[str] = set()

    # Ability files: unique job (case-insensitive — generated prefab folders
    # are Unity asset paths), unique ids and names
    seen_ability_jobs: set[str] = set()
    for file in json_files(ABILITY_DIR):
        data = load(file)
        if data is None:
            continue
        job = data.get("job") or ""
        if not job:
            errors.append(f"{file.name}: missing 'job' name")
            continue
        if job.lower() in seen_ability_jobs:
            errors.append(f"{file.name}: duplicate AbilityData file for job '{job}'")
            continue
        seen_ability_jobs.add(job.lower())

        ids, names = set(), set()
        # Ability names become '{name}.prefab' asset paths — duplicate
        # detection is case-insensitive; catalog membership keeps exact names
        names_ci: set[str] = set()
        ability_ids_by_job[job] = ids
        ability_names_by_job[job] = names
        for ability in data.get("abilities") or []:
            aid, name = ability.get("id") or "", ability.get("name") or ""
            if not aid:
                errors.append(f"{job}: ability '{name}' has no stable id")
            elif aid in all_ability_ids:
                errors.append(f"{job}: duplicate ability id '{aid}'")
            else:
                all_ability_ids.add(aid)
                ids.add(aid)
            if not name:
                errors.append(f"{job}: ability '{aid}' has no display name")
            elif name.lower() in names_ci:
                errors.append(f"{job}: duplicate ability name '{name}' (asset paths are case-insensitive)")
            else:
                names_ci.add(name.lower())
                names.add(name)

    # Catalogs: unique names; every entry must be an ability name of its job
    catalog_names: set[str] = set()
    seen_catalogs: set[str] = set()
    for file in json_files(CATALOG_DIR):
        data = load(file)
        if data is None:
            continue
        catalog = data.get("catalogName") or ""
        if catalog.lower() in seen_catalogs:
            errors.append(f"{file.name}: duplicate CatalogData file for catalog '{catalog}'")
            continue
        seen_catalogs.add(catalog.lower())
        catalog_names.add(catalog)
        job_names = ability_names_by_job.get(catalog)
        if job_names is None:
            errors.append(f"catalog '{catalog}': no AbilityData file defines job '{catalog}'")
            continue
        for category in data.get("categories") or []:
            for entry in category.get("entries") or []:
                if entry not in job_names:
                    errors.append(f"catalog '{catalog}': entry '{entry}' is not an ability of that job")

    # Jobs, first pass: collect resolved ids for prerequisite validation.
    # Ids become 'Jobs/{id}.asset' paths — duplicate detection is
    # case-insensitive, while prerequisite matching stays exact
    job_files = []
    job_ids: set[str] = set()
    job_ids_ci: set[str] = set()
    for file in json_files(JOB_DIR):
        data = load(file)
        if data is None:
            continue
        label = data.get("jobName") or file.name
        jid = data.get("id") or slug(data.get("jobName") or "")
        if jid.lower() in job_ids_ci:
            errors.append(f"{label}: duplicate job id '{jid}' (asset paths are case-insensitive)")
        else:
            job_ids_ci.add(jid.lower())
            job_ids.add(jid)
        job_files.append((label, data))

    # Jobs, second pass: catalog references, unlocks, curves, prerequisites
    for label, data in job_files:
        catalog = data.get("abilityCatalogName") or ""
        if not catalog:
            errors.append(f"{label}: missing abilityCatalogName")
        else:
            if catalog not in catalog_names:
                errors.append(f"{label}: abilityCatalogName '{catalog}' has no CatalogData file")
            if catalog not in ability_ids_by_job:
                errors.append(f"{label}: abilityCatalogName '{catalog}' has no AbilityData file")

        curve = data.get("jpRequirements") or []
        if len(curve) != JP_THRESHOLD_COUNT:
            errors.append(f"{label}: jpRequirements must have exactly {JP_THRESHOLD_COUNT} entries (levels 2-8), found {len(curve)}")
        else:
            for i, value in enumerate(curve):
                if value <= 0:
                    errors.append(f"{label}: jpRequirements[{i}] = {value} must be positive")
                elif i > 0 and value <= curve[i - 1]:
                    errors.append(f"{label}: jpRequirements[{i}] = {value} must be strictly greater than {curve[i - 1]}")

        own_ids = ability_ids_by_job.get(catalog, set())
        for unlock in data.get("abilityUnlocks") or []:
            name = unlock.get("abilityName") or ""
            aid = unlock.get("abilityId") or f"{slug(catalog or label)}.{slug(name)}"
            if aid not in own_ids:
                errors.append(f"{label}: unlock '{name}' resolves to id '{aid}' which its AbilityData does not define")
            else:
                unlocked_ability_ids.add(aid)
            level = unlock.get("unlockAtJobLevel", 0)
            if not 1 <= level <= MAX_JOB_LEVEL:
                errors.append(f"{label}: unlock '{name}' at job level {level}, outside 1-{MAX_JOB_LEVEL}")
            if unlock.get("jpCost", 0) < 0:
                errors.append(f"{label}: unlock '{name}' has negative jpCost {unlock.get('jpCost')}")

        for prereq in data.get("prerequisites") or []:
            required = prereq.get("requiredJobId") or slug(prereq.get("requiredJobName") or "")
            if required not in job_ids:
                errors.append(f"{label}: prerequisite '{prereq.get('requiredJobName')}' resolves to job id '{required}' which no JobData defines")
            level = prereq.get("requiredLevel", 0)
            if not 1 <= level <= MAX_JOB_LEVEL:
                errors.append(f"{label}: prerequisite '{prereq.get('requiredJobName')}' requires level {level}, outside 1-{MAX_JOB_LEVEL}")

    # Recipe-only abilities (nothing unlocks them) are worth a look, not a failure
    for aid in sorted(all_ability_ids - unlocked_ability_ids):
        warnings.append(f"ability '{aid}' is not unlockable by any job (recipe-only?)")

    for warning in warnings:
        print(f"WARNING: {warning}")
    for error in errors:
        print(f"ERROR: {error}")
    if errors:
        print(f"Content validation FAILED with {len(errors)} error(s).")
        return 1
    print(f"Content validation passed ({len(warnings)} warning(s)).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
