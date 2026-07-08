#!/usr/bin/env python3
"""
pg_to_sqlite_json.py
====================
Converts a PostgreSQL COPY-format dump into SQLite INSERT statements
that match the SqliteProtoRepository<T> JSON schema used by
Heracles.Indoor.SqliteGrpcServer.

Usage:
	python pg_to_sqlite_json.py <path_to_dump.sql> [<output.sql>]

	If <output.sql> is omitted the result is written to stdout.

Design notes:
  - The SQLite repository stores each protobuf message as Protobuf-JSON
	in a TEXT column called `data`, alongside an INTEGER PRIMARY KEY `id`.
  - Tables registered with hasParentId=true also have a `parent_id` column.
  - Protobuf-JSON rules (Google.Protobuf C# defaults):
	  * int64 / uint64  -> decimal string  e.g. "42"
	  * int32 / float / double -> plain JSON number
	  * bool -> true / false
	  * enum -> string value name
	  * google.protobuf.Timestamp -> RFC-3339 string  e.g. "2026-01-13T16:17:43.962Z"
	  * unset optional fields -> omitted
	  * default-valued non-optional fields -> omitted
  - PostgreSQL bytea columns (hex-encoded as \\xHH…) are decoded to UTF-8
	strings; an empty \\x becomes an empty string (omitted when optional).
"""

import re
import sys
import json
from pathlib import Path

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def to_camel(snake: str) -> str:
	parts = snake.split("_")
	return parts[0] + "".join(p.title() for p in parts[1:])


def pg_ts_to_rfc3339(ts: str) -> str:
	"""'2026-01-13 16:17:43.962'  ->  '2026-01-13T16:17:43.962Z'"""
	ts = ts.strip()
	# date-only  e.g. '1998-11-14'
	if re.fullmatch(r"\d{4}-\d{2}-\d{2}", ts):
		return ts + "T00:00:00Z"
	return ts.replace(" ", "T") + "Z"


def decode_bytea(val: str) -> str | None:
	"""Decode PostgreSQL hex bytea '\\xHH...' to a UTF-8 string.
	Returns None for an empty \\x (treated as unset).
	"""
	if val == "\\x" or val == "":
		return None
	if val.startswith("\\x"):
		raw = bytes.fromhex(val[2:])
		return raw.decode("utf-8", errors="replace")
	return val


def json_str(v: str) -> str:
	"""Wrap a Python string as a JSON string literal."""
	return json.dumps(v, ensure_ascii=False)


# ---------------------------------------------------------------------------
# Column type tags used in TABLE_DEFS
#
# 'id'        - int64; becomes both SQLite id column AND "id" JSON string
# 'int64'     - int64 FK or other big int; JSON string
# 'int32'     - int32; JSON number
# 'float'     - float32; JSON number
# 'double'    - float64; JSON number
# 'bool'      - optional bool; JSON true/false (omit if NULL)
# 'bool_req'  - non-optional bool; JSON true/false, omit if false (default)
# 'enum'      - string enum value (kept as-is from PG)
# 'string'    - plain string
# 'bytea'     - hex bytea decoded to UTF-8 string
# 'timestamp' - PG timestamp -> RFC-3339
# 'date'      - PG date-only -> RFC-3339 midnight UTC
# 'skip'      - column present in PG dump but not in proto (ignored)
# ---------------------------------------------------------------------------

# parent_id column name for every hasParentId=true table.
# Value is the name of the *pg column* whose value maps to parent_id.
PARENT_ID_COL = {
	"diagnosis":               "patient_id",
	"visit":                   "patient_id",
	"simulation":              "diagnosis_id",
	"prescription":            "simulation_id",
	"plan":                    "prescription_id",
	"treatment_device":        "simulation_id",
	"position":                "simulation_id",
	"treatment_field":         "plan_id",
	"actual_treatment_field":  "treatment_id",
	"emission_treatment_field":"actual_treatment_field_id",
	"series":                  "diagnosis_id",
	"photo":                   "diagnosis_id",
	"collimator":              "collimator_configuration_id",
}

# SQLite table name for each PostgreSQL table name.
SQLITE_TABLE = {
	"actual_treatment_field":  "actual_treatment_fields",
	"coil_configuration":      "coil_configurations",
	"collimator":              "collimators",
	"collimator_configuration":"collimator_configurations",
	"correction_matrix":       "correction_matrices",
	"diagnosis":               "diagnoses",
	"document":                None,          # no SQLite counterpart
	"emission_treatment_field":"emission_treatment_fields",
	"head":                    "heads",
	"heater_current_config":   "heater_current_configs",
	"intensity":               "intensities",
	"output_factor":           "output_factors",
	"patient":                 "patients",
	"photo":                   "photos",
	"plan":                    "plans",
	"position":                "positions",
	"prescription":            "prescriptions",
	"preset_configuration":    "preset_configurations",
	"qcsample":                "qcsamples",
	"qcsample_field":          "qcsample_fields",
	"reference_field":         "reference_fields",
	"robot_sequence":          "robot_sequences",
	"robot_sequence_step":     "robot_sequence_steps",
	"robot_stored_position":   "robot_stored_positions",
	"role":                    "roles",
	"roles_permissions":       "roles_permissions",
	"safety_check":            "safety_checks",
	"series":                  "series",
	"settings":                "settings",
	"simulation":              "simulations",
	"treatment":               "treatments",
	"treatment_device":        "treatment_devices",
	"treatment_field":         "treatment_fields",
	"user_data":               "users",
	"user_role":               "user_roles",
	"visit":                   "visits",
	"warmup":                  "warmups",
}

# Per-table column definitions: list of (pg_col_name, proto_json_key, type).
# - pg_col_name  : matches the COPY column list from the dump
# - proto_json_key: camelCase key for the JSON blob (None -> derive from pg name)
# - type         : one of the tags above
#
# Columns tagged 'skip' are read but not emitted.
# 'id' columns are emitted both as the SQLite id param and inside the JSON blob.

COLUMNS = {
	# -------------------------------------------------------------------------
	"actual_treatment_field": [
		("id",                       "id",                    "id"),
		("treatment_id",             "treatmentId",           "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("field_name",               "fieldName",             "enum"),
		("actual_energy",            "actualEnergy",          "double"),
		("actual_dwell_time",        "actualDwellTime",       "double"),
		("actual_dose",              "actualDose",            "double"),
		("actual_current",           "actualCurrent",         "double"),
		("completed",                "completed",             "int32"),
		("resume_partial",           "resumePartial",         "int32"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"coil_configuration": [
		("id",                       "id",                    "id"),
		("preset_configuration_id",  "presetConfigurationId", "int64"),
		("create_date",              "createDate",            "timestamp"),
		("field_name",               "fieldName",             "enum"),
		("x_deflection_current",     "xDeflectionCurrent",   "float"),
		("y_deflection_current",     "yDeflectionCurrent",   "float"),
		("focus_current",            "focusCurrent",         "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"collimator": [
		("id",                       "id",                    "id"),
		("head_id",                  "headId",                "int64"),
		("collimator_configuration_id","collimatorConfigurationId","int64"),
		("create_date",              "createDate",            "timestamp"),
		("serial",                   "serial",                "string"),
		("is_active",                "isActive",              "bool"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"collimator_configuration": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("type",                     "type",                  "enum"),
		("energy",                   "energy",                "enum"),
		("power",                    "power",                 "int32"),
		("ssd",                      "ssd",                   "enum"),
		("referenced_dose_rate",     "referencedDoseRate",    "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"correction_matrix": [
		("id",                       "id",                    "id"),
		("preset_configuration_id",  "presetConfigurationId", "int64"),
		("create_date",              "createDate",            "timestamp"),
		("magnetometer_type",        "magnetometerType",      "enum"),
		("cm11",                     "cm11",                  "float"),
		("cm12",                     "cm12",                  "float"),
		("cm13",                     "cm13",                  "float"),
		("cm21",                     "cm21",                  "float"),
		("cm22",                     "cm22",                  "float"),
		("cm23",                     "cm23",                  "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"diagnosis": [
		("id",                       "id",                    "id"),
		("patient_id",               "patientId",             "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("site_name",                "siteName",              "string"),
		("site_location",            "siteLocation",          "enum"),
		("icd_code",                 "icdCode",               "enum"),
		("pathology",                "pathology",             "enum"),
		("sub_cell_type_one",        "subCellTypeOne",        "enum"),
		("sub_cell_type_two",        "subCellTypeTwo",        "enum"),
		("description",              "description",           "enum"),
		("archived",                 "archived",              "bool_req"),
		("referring",                "referring",             "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"document": [],  # no SQLite table – rows are skipped
	# -------------------------------------------------------------------------
	"emission_treatment_field": [
		("id",                       "id",                    "id"),
		("actual_treatment_field_id","actualTreatmentFieldId","int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("actual_dwell_time",        "actualDwellTime",       "double"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"head": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("serial",                   "serial",                "string"),
		("is_active",                "isActive",              "bool"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"heater_current_config": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("preset_configuration_id",  "presetConfigurationId", "int64"),
		("heater_current",           "heaterCurrent",         "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"intensity": [
		("id",                       "id",                    "id"),
		("qcsample_fields_id",       "qcsampleFieldsId",      "int64"),
		("create_date",              "createDate",            "timestamp"),
		("diode_name",               "diodeName",             "string"),
		("intensity",                "intensity",             "double"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"output_factor": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("preset_configuration_id",  "presetConfigurationId", "int64"),
		("field_name",               "fieldName",             "enum"),
		("factor",                   "factor",                "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"patient": [
		("id",                       "id",                    "id"),
		("creation_date",            "creationDate",          "timestamp"),
		("provider_id",              "providerId",            "string"),
		("picture",                  "picture",               "string"),
		("patient_id",               "patientId",             "bytea"),
		("patient_id_type",          "patientIdType",         "enum"),
		("mrn",                      "mrn",                   "bytea"),
		("first_name",               "firstName",             "bytea"),
		("middle_name",              "middleName",            "bytea"),
		("last_name",                "lastName",              "bytea"),
		("sex",                      "sex",                   "enum"),
		("dob",                      "dob",                   "date"),
		("address",                  "address",               "bytea"),
		("city",                     "city",                  "bytea"),
		("state",                    "state",                 "bytea"),
		("zip",                      "zip",                   "bytea"),
		("country",                  "country",               "bytea"),
		("phone",                    "phone",                 "bytea"),
		("email",                    "email",                 "bytea"),
		("ethnicity",                "ethnicity",             "bytea"),
		("race",                     "race",                  "bytea"),
		("status",                   "status",                "enum"),
		("notes",                    "notes",                 "string"),
		("synced",                   None,                    "skip"),
		("sig",                      None,                    "skip"),
		("sig64",                    None,                    "skip"),
		("keyId",                    None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"photo": [
		("id",                       "id",                    "id"),
		("diagnosis_id",             "diagnosisId",           "int64"),
		("visit_id",                 "visitId",               "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("description",              "description",           "string"),
		("template_type",            "templateType",          "enum"),
		("photo_type",               "photoType",             "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"plan": [
		("id",                       "id",                    "id"),
		("prescription_id",          "prescriptionId",        "int64"),
		("approved_by",              "approvedBy",            "string"),
		("name",                     "name",                  "string"),
		("origin_series_id",         "originSeriesId",        "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("target_type",              "targetType",            "enum"),
		("status",                   "status",                "enum"),
		("treatment_loading_state",  "treatmentLoadingState", "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"position": [
		("id",                       "id",                    "id"),
		("simulation_id",            "simulationId",          "int64"),
		("create_date",              "createDate",            "timestamp"),
		("patient_position",         "patientPosition",       "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"prescription": [
		("id",                       "id",                    "id"),
		("simulation_id",            "simulationId",          "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("txs_per_week",             "txsPerWeek",            "int32"),
		("energy",                   "energy",                "enum"),
		("dwell_time",               "dwellTime",             "double"),
		("tdf",                      "tdf",                   "enum"),
		("min_tdf",                  "minTdf",                "enum"),
		("daily_dose",               "dailyDose",             "float"),
		("number_of_fxs",            "numberOfFxs",           "int32"),
		("total_dose",               "totalDose",             "float"),
		("status",                   "status",                "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"preset_configuration": [
		("id",                       "id",                    "id"),
		("collimator_configuration_id","collimatorConfigurationId","int64"),
		("create_date",              "createDate",            "timestamp"),
		("preset_name",              "presetName",            "string"),
		("is_default",               "isDefault",             "bool"),
		("is_active",                "isActive",              "bool"),
		("approved_by",              "approvedBy",            "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"qcsample": [
		("id",                       "id",                    "id"),
		("collimator_configuration_id","collimatorConfigurationId","int64"),
		("performed_by",             "performedBy",           "string"),
		("create_date",              "createDate",            "timestamp"),
		("emission_current",         "emissionCurrent",       "float"),
		("heater_current",           "heaterCurrent",         "float"),
		("duration",                 "duration",              "float"),
		("referenced",               "referenced",            "bool"),
		("approved_by",              "approvedBy",            "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"qcsample_field": [
		("id",                       "id",                    "id"),
		("qcsample_id",              "qcsampleId",            "int64"),
		("create_date",              "createDate",            "timestamp"),
		("field",                    "field",                 "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"reference_field": [
		("id",                       "id",                    "id"),
		("preset_configuration_id",  "presetConfigurationId", "int64"),
		("create_date",              "createDate",            "timestamp"),
		("magnetometer_type",        "magnetometerType",      "enum"),
		("rf11",                     "rf11",                  "float"),
		("rf21",                     "rf21",                  "float"),
		("rf31",                     "rf31",                  "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"robot_sequence": [
		("id",                       "id",                    "id"),
		("creation_date",            "creationDate",          "timestamp"),
		("sequences_name",           "sequencesName",         "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"robot_sequence_step": [
		("id",                       "id",                    "id"),
		("robot_sequence_id",        "robotSequenceId",       "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("step_index",               "stepIndex",             "int32"),
		("action",                   "action",                "string"),
		("value",                    "value",                 "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"robot_stored_position": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("position_name",            "positionName",          "string"),
		("j1",                       "j1",                    "float"),
		("j2",                       "j2",                    "float"),
		("j3",                       "j3",                    "float"),
		("j4",                       "j4",                    "float"),
		("j5",                       "j5",                    "float"),
		("j6",                       "j6",                    "float"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"role": [
		("id",                       "id",                    "id"),
		("creation_date",            "creationDate",          "timestamp"),
		("role_name",                "roleName",              "string"),
		("description",              "description",           "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"roles_permissions": [
		("id",                       "id",                    "id"),
		("role_id",                  "roleId",                "int64"),
		("permission",               "permission",            "enum"),
		("creation_date",            "creationDate",          "timestamp"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"safety_check": [
		("id",                       "id",                    "id"),
		("performed_by",             "performedBy",           "string"),
		("create_date",              "createDate",            "timestamp"),
		("energy",                   "energy",                "enum"),
		("duration",                 "duration",              "float"),
		("dose",                     "dose",                  "float"),
		("x_ray_light",              "xRayLight",             "bool"),
		("x_ray_sound",              "xRaySound",             "bool"),
		("door_interlock",           "doorInterlock",         "bool"),
		("e_stop",                   "eStop",                 "bool"),
		("s_stop",                   "sStop",                 "bool"),
		("live_video",               "liveVideo",             "bool"),
		("live_audio",               "liveAudio",             "bool"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"series": [
		("id",                       "id",                    "id"),
		("diagnosis_id",             "diagnosisId",           "int64"),
		("visit_id",                 "visitId",               "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("name",                     "name",                  "string"),
		("type",                     "type",                  "enum"),
		("location",                 "location",              "string"),
		("lesion_depth",             "lesionDepth",           "float"),
		("description",              "description",           "string"),
		("num_of_instances",         "numOfInstances",        "int32"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"settings": [
		("id",                       "id",                    "id"),
		("create_date",              "createDate",            "timestamp"),
		("device_serial",            "deviceSerial",          "string"),
		("record_and_verify_ip",     "recordAndVerifyIp",     "string"),
		("record_and_verify_port",   "recordAndVerifyPort",   "string"),
		("database_ip",              "databaseIp",            "string"),
		("database_port",            "databasePort",          "string"),
		("imaging_headcam_ip",       "imagingHeadcamIp",      "string"),
		("imaging_headcam_port",     "imagingHeadcamPort",    "string"),
		("treatment_headcam_ip",     "treatmentHeadcamIp",    "string"),
		("treatment_headcam_port",   "treatmentHeadcamPort",  "string"),
		("robotcam_ip",              "robotcamIp",            "string"),
		("robotcam_port",            "robotcamPort",          "string"),
		("gcb_telemetry_ip",         "gcbTelemetryIp",        "string"),
		("gcb_telemetry_port",       "gcbTelemetryPort",      "string"),
		("gcb_commands_ip",          "gcbCommandsIp",         "string"),
		("gcb_commands_port",        "gcbCommandsPort",       "string"),
		("robotic_ros_ip",           "roboticRosIp",          "string"),
		("robotic_ros_port",         "roboticRosPort",        "string"),
		("data_acquisition_ip",      "dataAcquisitionIp",     "string"),
		("data_acquisition_port",    "dataAcquisitionPort",   "string"),
		("dc_data_reconstruction_ip","dcDataReconstructionIp","string"),
		("dc_data_reconstruction_port","dcDataReconstructionPort","string"),
		("dc_data_progress_websocket_ip","dcDataProgressWebsocketIp","string"),
		("dc_data_progress_websocket_port","dcDataProgressWebsocketPort","string"),
		("dc_data_reconstruction_z_mq_ip","dcDataReconstructionZMqIp","string"),
		("dc_data_reconstruction_z_mq_port","dcDataReconstructionZMqPort","string"),
		("dc_database_ip",           "dcDatabaseIp",          "string"),
		("dc_database_port",         "dcDatabasePort",        "string"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"simulation": [
		("id",                       "id",                    "id"),
		("diagnosis_id",             "diagnosisId",           "int64"),
		("visit_id",                 "visitId",               "int64"),
		("performed_by",             "performedBy",           "string"),
		("creation_date",            "creationDate",          "timestamp"),
		("lesion_size_l",            "lesionSizeL",           "double"),
		("lesion_size_w",            "lesionSizeW",           "double"),
		("lesion_depth",             "lesionDepth",           "double"),
		("margin_size_l",            "marginSizeL",           "double"),
		("margin_size_w",            "marginSizeW",           "double"),
		("shield_size_l",            "shieldSizeL",           "double"),
		("shield_size_w",            "shieldSizeW",           "double"),
		("target_type",              "targetType",            "enum"),
		("setup_note",               "setupNote",             "string"),
		("status",                   "status",                "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"treatment": [
		("id",                       "id",                    "id"),
		("plan_id",                  "planId",                "int64"),
		("visit_id",                 "visitId",               "int64"),
		("performed_by",             "performedBy",           "string"),
		("creation_date",            "creationDate",          "timestamp"),
		("fraction",                 "fraction",              "int32"),
		("lesion_depth",             "lesionDepth",           "double"),
		("daily_dose",               "dailyDose",             "double"),
		("cumulative_dose",          "cumulativeDose",        "double"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"treatment_device": [
		("id",                       "id",                    "id"),
		("simulation_id",            "simulationId",          "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("device_name",              "deviceName",            "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"treatment_field": [
		("id",                       "id",                    "id"),
		("plan_id",                  "planId",                "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("field_name",               "fieldName",             "enum"),
		("energy",                   "energy",                "enum"),
		("dwell_time",               "dwellTime",             "double"),
		("calculated_dose",          "calculatedDose",        "double"),
		("current",                  "current",               "double"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	# PG table is user_data; SQLite table is users
	"user_data": [
		("id",                       "id",                    "id"),
		("creation_date",            "creationDate",          "timestamp"),
		("picture",                  "picture",               "string"),
		("first_name",               "firstName",             "string"),
		("middle_name",              "middleName",            "string"),
		("last_name",                "lastName",              "string"),
		("username",                 "username",              "string"),
		("password",                 "password",              "string"),
		("role",                     "role",                  "string"),
		("email_address",            "emailAddress",          "string"),
		("last_accessed",            "lastAccessed",          "timestamp"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"user_role": [
		("id",                       "id",                    "id"),
		("user_id",                  "userId",                "string"),
		("role_id",                  "roleId",                "int64"),
		("creation_date",            "creationDate",          "timestamp"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"visit": [
		("id",                       "id",                    "id"),
		("creation_date",            "creationDate",          "timestamp"),
		("patient_id",               "patientId",             "int64"),
		("type",                     "type",                  "enum"),
		("synced",                   None,                    "skip"),
	],
	# -------------------------------------------------------------------------
	"warmup": [
		("id",                       "id",                    "id"),
		("head_id",                  "headId",                "int64"),
		("create_date",              "createDate",            "timestamp"),
		("warmup_type",              "warmupType",            "enum"),
		("heater_current",           "heaterCurrent",         "float"),
		("synced",                   None,                    "skip"),
	],
}

# ---------------------------------------------------------------------------
# PostgreSQL COPY-format parser
# ---------------------------------------------------------------------------

def split_pg_row(line: str) -> list[str]:
	"""Split a tab-separated PostgreSQL COPY row into fields.
	Handles PG escape sequences: \\N (NULL), \\t (tab), \\n (newline), \\\\ (backslash).
	Does NOT further decode values beyond \\N; bytea hex is handled by decode_bytea().
	"""
	raw_fields = line.rstrip("\n").split("\t")
	result = []
	for f in raw_fields:
		if f == "\\N":
			result.append(None)
		else:
			# Unescape basic PG escape sequences
			f = f.replace("\\\\", "\x00BACKSLASH\x00")
			f = f.replace("\\t", "\t")
			f = f.replace("\\n", "\n")
			f = f.replace("\\r", "\r")
			f = f.replace("\x00BACKSLASH\x00", "\\")
			result.append(f)
	return result


def parse_copy_blocks(sql_text: str) -> dict[str, tuple[list[str], list[list[str | None]]]]:
	"""Return {pg_table_name: (columns, rows)} for every COPY block."""
	# Pattern:  COPY public.table_name (col1, col2, ...) FROM stdin;
	header_re = re.compile(
		r"COPY\s+(?:public\.)?\"?(\w+)\"?\s*\(([^)]+)\)\s+FROM\s+stdin\s*;",
		re.IGNORECASE,
	)
	result: dict[str, tuple[list[str], list[list[str | None]]]] = {}
	lines = sql_text.splitlines(keepends=True)
	i = 0
	while i < len(lines):
		m = header_re.search(lines[i])
		if m:
			table_name = m.group(1).strip('"')
			col_names = [c.strip().strip('"') for c in m.group(2).split(",")]
			rows: list[list[str | None]] = []
			i += 1
			while i < len(lines):
				row_line = lines[i]
				if row_line.rstrip("\n") == "\\.":
					i += 1
					break
				rows.append(split_pg_row(row_line))
				i += 1
			result[table_name] = (col_names, rows)
		else:
			i += 1
	return result


# ---------------------------------------------------------------------------
# Value converter
# ---------------------------------------------------------------------------

NULL = "\\N"  # sentinel (never appears as a real value after split_pg_row)


def convert_value(raw: str | None, typ: str, json_key: str) -> str | None:
	"""Convert a raw PostgreSQL string value to a JSON fragment (key: value).
	Returns None if the field should be omitted entirely.
	"""
	if raw is None:
		# NULL -> omit field (covers \\N)
		return None

	if typ == "skip":
		return None

	if typ == "id":
		# int64 -> JSON string
		return f'"{json_key}":"{raw}"'

	if typ == "int64":
		if raw == "" or raw == "\\N":
			return None
		return f'"{json_key}":"{raw}"'

	if typ == "int32":
		if raw == "" or raw == "\\N":
			return None
		try:
			int(raw)
		except ValueError:
			return None
		return f'"{json_key}":{raw}'

	if typ in ("float", "double"):
		if raw == "" or raw == "\\N":
			return None
		try:
			float(raw)
		except ValueError:
			return None
		return f'"{json_key}":{raw}'

	if typ == "bool":
		# optional bool: include whether true or false (field IS set)
		if raw == "t":
			return f'"{json_key}":true'
		if raw == "f":
			return f'"{json_key}":false'
		return None

	if typ == "bool_req":
		# non-optional bool: omit when false (default value)
		if raw == "t":
			return f'"{json_key}":true'
		return None  # false is default -> omit

	if typ == "enum":
		if raw == "" or raw == "\\N":
			return None
		return f'"{json_key}":{json.dumps(raw)}'

	if typ == "string":
		if raw == "" or raw == "\\N":
			return None
		return f'"{json_key}":{json.dumps(raw, ensure_ascii=False)}'

	if typ == "bytea":
		decoded = decode_bytea(raw)
		if decoded is None or decoded == "":
			return None
		return f'"{json_key}":{json.dumps(decoded, ensure_ascii=False)}'

	if typ == "timestamp":
		if raw == "" or raw == "\\N":
			return None
		rfc = pg_ts_to_rfc3339(raw)
		return f'"{json_key}":"{rfc}"'

	if typ == "date":
		if raw == "" or raw == "\\N":
			return None
		rfc = pg_ts_to_rfc3339(raw)
		return f'"{json_key}":"{rfc}"'

	raise ValueError(f"Unknown type tag: {typ!r}")


# ---------------------------------------------------------------------------
# SQL statement builder
# ---------------------------------------------------------------------------

def build_insert(sqlite_table: str, has_parent_id: bool,
				 row_id: int, parent_id: int | None, json_data: str) -> str:
	"""Return a single SQLite INSERT statement."""
	# Escape single-quotes inside JSON by doubling them (SQLite convention)
	safe_json = json_data.replace("'", "''")
	if has_parent_id and parent_id is not None:
		return (
			f"INSERT INTO {sqlite_table} (id, parent_id, data) VALUES "
			f"({row_id}, {parent_id}, '{safe_json}');"
		)
	return (
		f"INSERT INTO {sqlite_table} (id, data) VALUES "
		f"({row_id}, '{safe_json}');"
	)


# ---------------------------------------------------------------------------
# Main conversion
# ---------------------------------------------------------------------------

def convert(sql_text: str) -> str:
	blocks = parse_copy_blocks(sql_text)
	out: list[str] = []

	for pg_table, (col_names, rows) in blocks.items():
		sqlite_table = SQLITE_TABLE.get(pg_table)
		if sqlite_table is None:
			# Table has no SQLite counterpart (e.g. document)
			continue
		if not rows:
			continue

		col_defs = COLUMNS.get(pg_table)
		if col_defs is None:
			out.append(f"-- WARNING: no column mapping for table '{pg_table}' – skipped\n")
			continue

		# Build a lookup: pg_col_name -> (json_key, type)
		col_map: dict[str, tuple[str | None, str]] = {
			pg_col: (jkey, typ) for pg_col, jkey, typ in col_defs
		}

		has_parent_id = pg_table in PARENT_ID_COL
		parent_id_col = PARENT_ID_COL.get(pg_table)

		out.append(f"-- {pg_table} -> {sqlite_table}")
		out.append(f"DELETE FROM {sqlite_table};")

		for row in rows:
			if len(row) != len(col_names):
				# Malformed row; try to continue
				out.append(f"-- WARNING: column count mismatch in {pg_table}, row skipped")
				continue

			col_values: dict[str, str | None] = dict(zip(col_names, row))

			# Determine row id (always the first mapped column of type 'id')
			row_id_raw = col_values.get("id")
			if row_id_raw is None:
				out.append(f"-- WARNING: NULL id in {pg_table}, row skipped")
				continue
			row_id = int(row_id_raw)

			# Determine parent_id
			parent_id: int | None = None
			if has_parent_id and parent_id_col:
				pid_raw = col_values.get(parent_id_col)
				if pid_raw is not None:
					try:
						parent_id = int(pid_raw)
					except ValueError:
						parent_id = None

			# Build JSON object
			fragments: list[str] = []
			for pg_col in col_names:
				if pg_col not in col_map:
					# Column in dump but not in our definition -> skip with warning
					continue
				json_key, typ = col_map[pg_col]
				if typ == "skip" or json_key is None:
					continue
				raw_val = col_values[pg_col]
				fragment = convert_value(raw_val, typ, json_key)
				if fragment is not None:
					fragments.append(fragment)

			json_data = "{" + ",".join(fragments) + "}"
			out.append(build_insert(sqlite_table, has_parent_id, row_id, parent_id, json_data))

		out.append("")

	return "\n".join(out)


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main() -> None:
	args = sys.argv[1:]
	if not args:
		print(__doc__)
		sys.exit(1)

	input_path = Path(args[0])
	if not input_path.exists():
		print(f"Error: file not found: {input_path}", file=sys.stderr)
		sys.exit(1)

	sql_text = input_path.read_text(encoding="utf-8", errors="replace")
	result = convert(sql_text)

	if len(args) >= 2:
		out_path = Path(args[1])
		out_path.write_text(result, encoding="utf-8")
		print(f"Written to {out_path}", file=sys.stderr)
	else:
		print(result)


if __name__ == "__main__":
	main()
