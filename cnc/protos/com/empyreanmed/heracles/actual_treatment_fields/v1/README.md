# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/actual_treatment_fields/v1/actual_treatment_field.proto](#com_empyreanmed_heracles_actual_treatment_fields_v1_actual_treatment_field-proto)
    - [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField)
  
- [com/empyreanmed/heracles/actual_treatment_fields/v1/actual_treatment_field_service.proto](#com_empyreanmed_heracles_actual_treatment_fields_v1_actual_treatment_field_service-proto)
    - [CreateActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldRequest)
    - [CreateActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldResponse)
    - [DeleteActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldRequest)
    - [DeleteActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldResponse)
    - [GetActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldRequest)
    - [GetActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldResponse)
    - [ListActualTreatmentFieldsRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsRequest)
    - [ListActualTreatmentFieldsResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsResponse)
    - [UpdateActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldRequest)
    - [UpdateActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldResponse)
  
    - [ActualTreatmentFieldService](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentFieldService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_actual_treatment_fields_v1_actual_treatment_field-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/actual_treatment_fields/v1/actual_treatment_field.proto



<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField"></a>

### ActualTreatmentField
Represents an actual treatment field used in a treatment.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Actual treatment field id, globally unique |
| treatment_id | [int64](#int64) | optional | Treatment id associated with the actual treatment field |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Actual treatment field creation date |
| field_name | [com.empyreanmed.heracles.enums.v1.FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME) | optional | Name of the actual treatment field |
| actual_energy | [double](#double) | optional | Actual energy of the actual treatment field |
| actual_dwell_time | [double](#double) | optional | Actual dwell time of the actual treatment field |
| actual_dose | [double](#double) | optional | Actual dose delivered by the actual treatment field |
| actual_current | [double](#double) | optional | Actual current of the actual treatment field |
| completed | [int32](#int32) | optional | Completed dose |
| resume_partial | [int32](#int32) | optional | Resume partial treatment |





 

 

 

 



<a name="com_empyreanmed_heracles_actual_treatment_fields_v1_actual_treatment_field_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/actual_treatment_fields/v1/actual_treatment_field_service.proto



<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldRequest"></a>

### CreateActualTreatmentFieldRequest
Request message for creating a new actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) | optional | Details of the actual treatment field to create. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldResponse"></a>

### CreateActualTreatmentFieldResponse
Response message with the created actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) |  | The actual treatment field that was created. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldRequest"></a>

### DeleteActualTreatmentFieldRequest
Request message for deleting a actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field_id | [int64](#int64) | optional | The ID of the actual treatment field to delete. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldResponse"></a>

### DeleteActualTreatmentFieldResponse
An empty response message for `DeleteActualTreatmentField`.






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldRequest"></a>

### GetActualTreatmentFieldRequest
Request message for fetching a single actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field_id | [int64](#int64) | optional | The ID of the actual treatment field to fetch. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldResponse"></a>

### GetActualTreatmentFieldResponse
Response message with the fetched actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) |  | The actual treatment field with the provided ID. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsRequest"></a>

### ListActualTreatmentFieldsRequest
Request message for listing actual treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field_id | [int64](#int64) | optional | The ID of the treatment_field for which to list actual treatment fields. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsResponse"></a>

### ListActualTreatmentFieldsResponse
Response message with the listed actual treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_fields | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) | repeated | The actual treatment fields matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldRequest"></a>

### UpdateActualTreatmentFieldRequest
Request message for updating an existing actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) | optional | The actual treatment field to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the actual treatment field to update. |






<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldResponse"></a>

### UpdateActualTreatmentFieldResponse
Response message with the updated actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field | [ActualTreatmentField](#com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentField) |  | The updated actual treatment field. |





 

 

 


<a name="com-empyreanmed-heracles-actual_treatment_fields-v1-ActualTreatmentFieldService"></a>

### ActualTreatmentFieldService
Performs operations on actual treatment fields.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListActualTreatmentFields | [ListActualTreatmentFieldsRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsRequest) | [ListActualTreatmentFieldsResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-ListActualTreatmentFieldsResponse) | Lists actual treatment fields for a given plan. |
| GetActualTreatmentField | [GetActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldRequest) | [GetActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-GetActualTreatmentFieldResponse) | Returns a single actual treatment field. |
| CreateActualTreatmentField | [CreateActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldRequest) | [CreateActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-CreateActualTreatmentFieldResponse) | Creates a new actual treatment field for a plan. |
| UpdateActualTreatmentField | [UpdateActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldRequest) | [UpdateActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-UpdateActualTreatmentFieldResponse) | Updates an existing actual treatment field. |
| DeleteActualTreatmentField | [DeleteActualTreatmentFieldRequest](#com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldRequest) | [DeleteActualTreatmentFieldResponse](#com-empyreanmed-heracles-actual_treatment_fields-v1-DeleteActualTreatmentFieldResponse) | Deletes a actual treatment field. |

 



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

