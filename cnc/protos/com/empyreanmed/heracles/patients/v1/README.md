# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/patients/v1/patient.proto](#com_empyreanmed_heracles_patients_v1_patient-proto)
    - [Patient](#com-empyreanmed-heracles-patients-v1-Patient)
  
- [com/empyreanmed/heracles/patients/v1/patient_service.proto](#com_empyreanmed_heracles_patients_v1_patient_service-proto)
    - [CreatePatientRequest](#com-empyreanmed-heracles-patients-v1-CreatePatientRequest)
    - [CreatePatientResponse](#com-empyreanmed-heracles-patients-v1-CreatePatientResponse)
    - [DeletePatientRequest](#com-empyreanmed-heracles-patients-v1-DeletePatientRequest)
    - [DeletePatientResponse](#com-empyreanmed-heracles-patients-v1-DeletePatientResponse)
    - [GetPatientRequest](#com-empyreanmed-heracles-patients-v1-GetPatientRequest)
    - [GetPatientResponse](#com-empyreanmed-heracles-patients-v1-GetPatientResponse)
    - [ListPatientsRequest](#com-empyreanmed-heracles-patients-v1-ListPatientsRequest)
    - [ListPatientsResponse](#com-empyreanmed-heracles-patients-v1-ListPatientsResponse)
    - [SearchPatientsRequest](#com-empyreanmed-heracles-patients-v1-SearchPatientsRequest)
    - [SearchPatientsResponse](#com-empyreanmed-heracles-patients-v1-SearchPatientsResponse)
    - [UpdatePatientRequest](#com-empyreanmed-heracles-patients-v1-UpdatePatientRequest)
    - [UpdatePatientResponse](#com-empyreanmed-heracles-patients-v1-UpdatePatientResponse)
  
    - [PatientService](#com-empyreanmed-heracles-patients-v1-PatientService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_patients_v1_patient-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/patients/v1/patient.proto



<a name="com-empyreanmed-heracles-patients-v1-Patient"></a>

### Patient
Patients receive treatments and have modalities and treatment plans associated with them.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Patient id, globally unique |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Patient creation date |
| provider_id | [string](#string) | optional | email of patient&#39;s provider. |
| picture | [string](#string) | optional | path to patient&#39;s profile picture in device&#39;s storage. |
| patient_id | [string](#string) | optional | Patient identification number, accompanied by a type. |
| patient_id_type | [com.empyreanmed.heracles.enums.v1.PATIENTIDTYPE](#com-empyreanmed-heracles-enums-v1-PATIENTIDTYPE) | optional | The type of patient identification, e.g Social Security / Passport number. Should be set from a user interface when setting a `patient_id`, and it can be used to disambiguate patients that may have the same `patient_id` value, but that represent different identification schemes in different countries for example. |
| mrn | [string](#string) | optional | Medical Record Number |
| first_name | [string](#string) | optional | First name |
| middle_name | [string](#string) | optional | Middle name |
| last_name | [string](#string) | optional | Last name |
| sex | [com.empyreanmed.heracles.enums.v1.SEXTYPE](#com-empyreanmed-heracles-enums-v1-SEXTYPE) | optional | Sex |
| dob | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Birth date |
| address | [string](#string) | optional | Postal address line |
| city | [string](#string) | optional | City |
| state | [string](#string) | optional | State |
| zip | [string](#string) | optional | Zip code |
| country | [string](#string) | optional | Country |
| phone | [string](#string) | optional | Phone number |
| email | [string](#string) | optional | Email address |
| ethnicity | [string](#string) | optional | Ethnicity |
| race | [string](#string) | optional | Race, e.g White/Caucasian, African-American/Black, Hispanic/Latino etc |
| notes | [string](#string) | optional | Notes about patient |
| status | [com.empyreanmed.heracles.enums.v1.PATIENTSTATUS](#com-empyreanmed-heracles-enums-v1-PATIENTSTATUS) | optional | Status for Patient |





 

 

 

 



<a name="com_empyreanmed_heracles_patients_v1_patient_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/patients/v1/patient_service.proto



<a name="com-empyreanmed-heracles-patients-v1-CreatePatientRequest"></a>

### CreatePatientRequest
Request message for creating a new patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) | optional | The patient to be created. |






<a name="com-empyreanmed-heracles-patients-v1-CreatePatientResponse"></a>

### CreatePatientResponse
Response message with the created patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) | optional | The patient that was created. |






<a name="com-empyreanmed-heracles-patients-v1-DeletePatientRequest"></a>

### DeletePatientRequest
Request message for deleting a patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the patient to be deleted. |






<a name="com-empyreanmed-heracles-patients-v1-DeletePatientResponse"></a>

### DeletePatientResponse
An empty response message for `DeletePatient`.






<a name="com-empyreanmed-heracles-patients-v1-GetPatientRequest"></a>

### GetPatientRequest
Request message for fetching a single patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the patient to be returned. |






<a name="com-empyreanmed-heracles-patients-v1-GetPatientResponse"></a>

### GetPatientResponse
Response message for fetching a single patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) |  | The patient with the ID provided in the request. |






<a name="com-empyreanmed-heracles-patients-v1-ListPatientsRequest"></a>

### ListPatientsRequest
Request message for listing patients.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of patients to return. The service may return fewer than this value. If unset or zero, all patients will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListPatients` call. Provide this to retrieve the subsequent page.

When paginating, all other parameters provided to `ListPatients` must match the call that provided the page token. |






<a name="com-empyreanmed-heracles-patients-v1-ListPatientsResponse"></a>

### ListPatientsResponse
Response message with the listed patients.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patients | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) | repeated | The patients matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-patients-v1-SearchPatientsRequest"></a>

### SearchPatientsRequest
Request message for searching patients.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| first_name | [string](#string) | optional | Optional first name for searching |
| middle_name | [string](#string) | optional | Optional middle name for searching |
| last_name | [string](#string) | optional | Optional last name for searching |






<a name="com-empyreanmed-heracles-patients-v1-SearchPatientsResponse"></a>

### SearchPatientsResponse
Response message with the listed patients.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patients | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) | repeated | The patients matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-patients-v1-UpdatePatientRequest"></a>

### UpdatePatientRequest
Request message for updating an existing patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) | optional | The patient to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the patient to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-patients-v1-UpdatePatientResponse"></a>

### UpdatePatientResponse
Response message with the updated patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient | [Patient](#com-empyreanmed-heracles-patients-v1-Patient) |  | The updated patient. |





 

 

 


<a name="com-empyreanmed-heracles-patients-v1-PatientService"></a>

### PatientService
Performs CRUD operations on patients.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPatients | [ListPatientsRequest](#com-empyreanmed-heracles-patients-v1-ListPatientsRequest) | [ListPatientsResponse](#com-empyreanmed-heracles-patients-v1-ListPatientsResponse) | Lists patients matching request parameters. |
| GetPatient | [GetPatientRequest](#com-empyreanmed-heracles-patients-v1-GetPatientRequest) | [GetPatientResponse](#com-empyreanmed-heracles-patients-v1-GetPatientResponse) | Returns a single patient. |
| CreatePatient | [CreatePatientRequest](#com-empyreanmed-heracles-patients-v1-CreatePatientRequest) | [CreatePatientResponse](#com-empyreanmed-heracles-patients-v1-CreatePatientResponse) | Creates a new patient. |
| UpdatePatient | [UpdatePatientRequest](#com-empyreanmed-heracles-patients-v1-UpdatePatientRequest) | [UpdatePatientResponse](#com-empyreanmed-heracles-patients-v1-UpdatePatientResponse) | Updates a single patient. |
| DeletePatient | [DeletePatientRequest](#com-empyreanmed-heracles-patients-v1-DeletePatientRequest) | [DeletePatientResponse](#com-empyreanmed-heracles-patients-v1-DeletePatientResponse) | Deletes a single patient. |
| SearchPatients | [SearchPatientsRequest](#com-empyreanmed-heracles-patients-v1-SearchPatientsRequest) | [SearchPatientsResponse](#com-empyreanmed-heracles-patients-v1-SearchPatientsResponse) | Searches for patients based on name parameters and return the list patient response. |

 



## Scalar Value Types

| .proto Type | Notes | C++ | Java | Python | Go | C# | PHP | Ruby |
| ----------- | ----- | --- | ---- | ------ | -- | -- | --- | ---- |
| <a name="double" /> double |  | double | double | float | float64 | double | float | Float |
| <a name="float" /> float |  | float | float | float | float32 | float | float | Float |
| <a name="int32" /> int32 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint32 instead. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="int64" /> int64 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint64 instead. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="uint32" /> uint32 | Uses variable-length encoding. | uint32 | int | int/long | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="uint64" /> uint64 | Uses variable-length encoding. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum or Fixnum (as required) |
| <a name="sint32" /> sint32 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int32s. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sint64" /> sint64 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int64s. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="fixed32" /> fixed32 | Always four bytes. More efficient than uint32 if values are often greater than 2^28. | uint32 | int | int | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="fixed64" /> fixed64 | Always eight bytes. More efficient than uint64 if values are often greater than 2^56. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum |
| <a name="sfixed32" /> sfixed32 | Always four bytes. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sfixed64" /> sfixed64 | Always eight bytes. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="bool" /> bool |  | bool | boolean | boolean | bool | bool | boolean | TrueClass/FalseClass |
| <a name="string" /> string | A string must always contain UTF-8 encoded or 7-bit ASCII text. | string | String | str/unicode | string | string | string | String (UTF-8) |
| <a name="bytes" /> bytes | May contain any arbitrary sequence of bytes. | string | ByteString | str | []byte | ByteString | string | String (ASCII-8BIT) |

