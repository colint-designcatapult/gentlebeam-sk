# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/diagnoses/v1/diagnosis.proto](#com_empyreanmed_heracles_diagnoses_v1_diagnosis-proto)
    - [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis)
  
- [com/empyreanmed/heracles/diagnoses/v1/diagnosis_service.proto](#com_empyreanmed_heracles_diagnoses_v1_diagnosis_service-proto)
    - [CreateDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisRequest)
    - [CreateDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisResponse)
    - [DeleteDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisRequest)
    - [DeleteDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisResponse)
    - [GetDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisRequest)
    - [GetDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisResponse)
    - [ListDiagnosesRequest](#com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesRequest)
    - [ListDiagnosesResponse](#com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesResponse)
    - [UpdateDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisRequest)
    - [UpdateDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisResponse)
  
    - [DiagnosisService](#com-empyreanmed-heracles-diagnoses-v1-DiagnosisService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_diagnoses_v1_diagnosis-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/diagnoses/v1/diagnosis.proto



<a name="com-empyreanmed-heracles-diagnoses-v1-Diagnosis"></a>

### Diagnosis
Diagnoses represent a diagnosis of a pathology in a physical region (or &#34;site&#34;) on a patient.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Diagnosis id, globally unique |
| patient_id | [int64](#int64) | optional | The diagnosed patient |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Diagnosis creation date |
| site_name | [string](#string) | optional | The name of the physical site on the patient&#39;s body where the pathology was diagnosed |
| site_location | [com.empyreanmed.heracles.enums.v1.SITELOCATION](#com-empyreanmed-heracles-enums-v1-SITELOCATION) | optional | Diagnosed site location |
| icd_code | [com.empyreanmed.heracles.enums.v1.ICDCODE](#com-empyreanmed-heracles-enums-v1-ICDCODE) | optional | IDC code for Diagnoses |
| pathology | [com.empyreanmed.heracles.enums.v1.PATHOLOGY](#com-empyreanmed-heracles-enums-v1-PATHOLOGY) | optional | The diagnosed pathology |
| sub_cell_type_one | [com.empyreanmed.heracles.enums.v1.CELLTYPE](#com-empyreanmed-heracles-enums-v1-CELLTYPE) | optional | The diagnosed sub_cell_type_one |
| sub_cell_type_two | [com.empyreanmed.heracles.enums.v1.CELLTYPE](#com-empyreanmed-heracles-enums-v1-CELLTYPE) | optional | The diagnosed sub_cell_type_two |
| description | [com.empyreanmed.heracles.enums.v1.DESCRIPTION](#com-empyreanmed-heracles-enums-v1-DESCRIPTION) | optional | The diagnosis description |
| archived | [bool](#bool) |  | Whether the record is archived. Default is false. |
| referring | [string](#string) | optional | The name of the referring provider |





 

 

 

 



<a name="com_empyreanmed_heracles_diagnoses_v1_diagnosis_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/diagnoses/v1/diagnosis_service.proto



<a name="com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisRequest"></a>

### CreateDiagnosisRequest
Request message for creating a new diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) | optional | The diagnosis to be created. |






<a name="com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisResponse"></a>

### CreateDiagnosisResponse
Response message with the created diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) |  | The diagnosis that was created. |






<a name="com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisRequest"></a>

### DeleteDiagnosisRequest
Request message for deleting a diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [uint64](#uint64) | optional | The ID of the diagnosis to be deleted. |






<a name="com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisResponse"></a>

### DeleteDiagnosisResponse
An empty response message for `DeleteDiagnosis`.






<a name="com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisRequest"></a>

### GetDiagnosisRequest
Request message for fetching a single diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the diagnosis to be returned. |






<a name="com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisResponse"></a>

### GetDiagnosisResponse
Response message for fetching a single diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) |  | The diagnosis with the ID provided in the request. |






<a name="com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesRequest"></a>

### ListDiagnosesRequest
Request message for listing diagnoses.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient_id | [int64](#int64) | optional | Filter results by a given patient. |






<a name="com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesResponse"></a>

### ListDiagnosesResponse
Response message with the listed diagnoses.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnoses | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) | repeated | The diagnoses matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisRequest"></a>

### UpdateDiagnosisRequest
Request message for updating an existing diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) | optional | The diagnosis to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the diagnosis to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisResponse"></a>

### UpdateDiagnosisResponse
Response message with the updated diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis | [Diagnosis](#com-empyreanmed-heracles-diagnoses-v1-Diagnosis) |  | The updated diagnosis. |





 

 

 


<a name="com-empyreanmed-heracles-diagnoses-v1-DiagnosisService"></a>

### DiagnosisService
Performs CRUD operations on diagnoses.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListDiagnoses | [ListDiagnosesRequest](#com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesRequest) | [ListDiagnosesResponse](#com-empyreanmed-heracles-diagnoses-v1-ListDiagnosesResponse) | Lists diagnoses matching request parameters. |
| GetDiagnosis | [GetDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisRequest) | [GetDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-GetDiagnosisResponse) | Returns a single diagnosis. |
| CreateDiagnosis | [CreateDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisRequest) | [CreateDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-CreateDiagnosisResponse) | Creates a new diagnosis. |
| UpdateDiagnosis | [UpdateDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisRequest) | [UpdateDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-UpdateDiagnosisResponse) | Updates a single diagnosis. |
| DeleteDiagnosis | [DeleteDiagnosisRequest](#com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisRequest) | [DeleteDiagnosisResponse](#com-empyreanmed-heracles-diagnoses-v1-DeleteDiagnosisResponse) | Deletes a single diagnosis. |

 



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

