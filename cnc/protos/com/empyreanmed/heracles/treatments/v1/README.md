# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/treatments/v1/treatment.proto](#com_empyreanmed_heracles_treatments_v1_treatment-proto)
    - [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment)
  
- [com/empyreanmed/heracles/treatments/v1/treatment_service.proto](#com_empyreanmed_heracles_treatments_v1_treatment_service-proto)
    - [CreateTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-CreateTreatmentRequest)
    - [CreateTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-CreateTreatmentResponse)
    - [DeleteTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-DeleteTreatmentRequest)
    - [DeleteTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-DeleteTreatmentResponse)
    - [GetTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-GetTreatmentRequest)
    - [GetTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-GetTreatmentResponse)
    - [ListTreatmentsByDiagnosisIdRequest](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdRequest)
    - [ListTreatmentsByDiagnosisIdResponse](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdResponse)
    - [ListTreatmentsByPlanIdRequest](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdRequest)
    - [ListTreatmentsByPlanIdResponse](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdResponse)
    - [UpdateTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-UpdateTreatmentRequest)
    - [UpdateTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-UpdateTreatmentResponse)
  
    - [TreatmentService](#com-empyreanmed-heracles-treatments-v1-TreatmentService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_treatments_v1_treatment-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatments/v1/treatment.proto



<a name="com-empyreanmed-heracles-treatments-v1-Treatment"></a>

### Treatment
Represents a treatment administered to a patient during a visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Treatment id, globally unique |
| plan_id | [int64](#int64) | optional | Plan id associated with the treatment |
| visit_id | [int64](#int64) | optional | Visit id associated with the treatment |
| performed_by | [string](#string) | optional | User who performed the treatment |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Treatment creation date |
| fraction | [int32](#int32) | optional | Number of fractions |
| lesion_depth | [double](#double) | optional | Depth of the lesion in millimeters |
| daily_dose | [double](#double) | optional | Daily dose in centi-gray |
| cumulative_dose | [double](#double) | optional | Cumulative dose in centi-gray |





 

 

 

 



<a name="com_empyreanmed_heracles_treatments_v1_treatment_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatments/v1/treatment_service.proto



<a name="com-empyreanmed-heracles-treatments-v1-CreateTreatmentRequest"></a>

### CreateTreatmentRequest
Request message for creating a new treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) | optional | Details of the treatment to create. |






<a name="com-empyreanmed-heracles-treatments-v1-CreateTreatmentResponse"></a>

### CreateTreatmentResponse
Response message with the created treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) | optional | The treatment that was created. |






<a name="com-empyreanmed-heracles-treatments-v1-DeleteTreatmentRequest"></a>

### DeleteTreatmentRequest
Request message for deleting a treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_id | [int64](#int64) | optional | The ID of the treatment to delete. |






<a name="com-empyreanmed-heracles-treatments-v1-DeleteTreatmentResponse"></a>

### DeleteTreatmentResponse
An empty response message for `DeleteTreatment`.






<a name="com-empyreanmed-heracles-treatments-v1-GetTreatmentRequest"></a>

### GetTreatmentRequest
Request message for fetching a single treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_id | [int64](#int64) | optional | The ID of the treatment to fetch. |






<a name="com-empyreanmed-heracles-treatments-v1-GetTreatmentResponse"></a>

### GetTreatmentResponse
Response message with the fetched treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) |  | The treatment with the provided ID. |






<a name="com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdRequest"></a>

### ListTreatmentsByDiagnosisIdRequest
Request message for listing treatments by diagnosis ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the diagnosis associated with the treatments to be returned. |
| page_size | [int32](#int32) | optional | The maximum number of treatments to return. The service may return fewer than this value. If unset or zero, all treatments will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListTreatmentsByDiagnosisId` call. Provide this to retrieve the subsequent page. |






<a name="com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdResponse"></a>

### ListTreatmentsByDiagnosisIdResponse
Response message with the listed treatments by diagnosis ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatments | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) | repeated | The treatments matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdRequest"></a>

### ListTreatmentsByPlanIdRequest
Request message for listing treatments by plan ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the plan associated with the treatments to be returned. |
| page_size | [int32](#int32) | optional | The maximum number of treatments to return. The service may return fewer than this value. If unset or zero, all treatments will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListTreatmentsByPlanId` call. Provide this to retrieve the subsequent page. |






<a name="com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdResponse"></a>

### ListTreatmentsByPlanIdResponse
Response message with the listed treatments by plan ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatments | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) | repeated | The treatments matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-treatments-v1-UpdateTreatmentRequest"></a>

### UpdateTreatmentRequest
Request message for updating an existing treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) | optional | The treatment to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the treatment to update. |






<a name="com-empyreanmed-heracles-treatments-v1-UpdateTreatmentResponse"></a>

### UpdateTreatmentResponse
Response message with the updated treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment | [Treatment](#com-empyreanmed-heracles-treatments-v1-Treatment) |  | The updated treatment. |





 

 

 


<a name="com-empyreanmed-heracles-treatments-v1-TreatmentService"></a>

### TreatmentService
Performs operations on treatments.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListTreatmentsByPlanId | [ListTreatmentsByPlanIdRequest](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdRequest) | [ListTreatmentsByPlanIdResponse](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByPlanIdResponse) | Lists treatments by plan ID. |
| ListTreatmentsByDiagnosisId | [ListTreatmentsByDiagnosisIdRequest](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdRequest) | [ListTreatmentsByDiagnosisIdResponse](#com-empyreanmed-heracles-treatments-v1-ListTreatmentsByDiagnosisIdResponse) | Lists treatments by diagnosis ID. |
| GetTreatment | [GetTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-GetTreatmentRequest) | [GetTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-GetTreatmentResponse) | Returns a single treatment. |
| CreateTreatment | [CreateTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-CreateTreatmentRequest) | [CreateTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-CreateTreatmentResponse) | Creates a new treatment for a visit. |
| UpdateTreatment | [UpdateTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-UpdateTreatmentRequest) | [UpdateTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-UpdateTreatmentResponse) | Updates an existing treatment. |
| DeleteTreatment | [DeleteTreatmentRequest](#com-empyreanmed-heracles-treatments-v1-DeleteTreatmentRequest) | [DeleteTreatmentResponse](#com-empyreanmed-heracles-treatments-v1-DeleteTreatmentResponse) | Deletes a treatment. |

 



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

