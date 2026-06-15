# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/treatment_fields/v1/treatment_field.proto](#com_empyreanmed_heracles_treatment_fields_v1_treatment_field-proto)
    - [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField)
  
- [com/empyreanmed/heracles/treatment_fields/v1/treatment_field_service.proto](#com_empyreanmed_heracles_treatment_fields_v1_treatment_field_service-proto)
    - [CreateBatchTreatmentFieldsRequest](#com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsRequest)
    - [CreateBatchTreatmentFieldsResponse](#com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsResponse)
    - [CreateTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldRequest)
    - [CreateTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldResponse)
    - [DeleteTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldRequest)
    - [DeleteTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldResponse)
    - [GetTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldRequest)
    - [GetTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldResponse)
    - [ListTreatmentFieldsRequest](#com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsRequest)
    - [ListTreatmentFieldsResponse](#com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsResponse)
    - [UpdateTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldRequest)
    - [UpdateTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldResponse)
  
    - [TreatmentFieldService](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentFieldService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_treatment_fields_v1_treatment_field-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatment_fields/v1/treatment_field.proto



<a name="com-empyreanmed-heracles-treatment_fields-v1-TreatmentField"></a>

### TreatmentField
Represents a treatment field within a treatment plan.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Treatment field id, globally unique |
| plan_id | [int64](#int64) | optional | Plan id associated with the treatment field |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Treatment field creation date |
| field_name | [com.empyreanmed.heracles.enums.v1.FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME) | optional | Name of the treatment field |
| energy | [com.empyreanmed.heracles.enums.v1.ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY) | optional | Energy of the treatment field |
| dwell_time | [double](#double) | optional | Dwell time of the treatment field |
| calculated_dose | [double](#double) | optional | Calculated dose for the treatment field |
| current | [double](#double) | optional | Current of the treatment field |





 

 

 

 



<a name="com_empyreanmed_heracles_treatment_fields_v1_treatment_field_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatment_fields/v1/treatment_field_service.proto



<a name="com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsRequest"></a>

### CreateBatchTreatmentFieldsRequest
Request message for creating a batch of new treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_fields | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) | repeated | Details of the treatment fields to create. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsResponse"></a>

### CreateBatchTreatmentFieldsResponse
Response message with the created treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_fields | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) | repeated | The treatment fields that were created. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldRequest"></a>

### CreateTreatmentFieldRequest
Request message for creating a new treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) | optional | Details of the treatment field to create. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldResponse"></a>

### CreateTreatmentFieldResponse
Response message with the created treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) |  | The treatment field that was created. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldRequest"></a>

### DeleteTreatmentFieldRequest
Request message for deleting a treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field_id | [int64](#int64) | optional | The ID of the treatment field to delete. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldResponse"></a>

### DeleteTreatmentFieldResponse
An empty response message for `DeleteTreatmentField`.






<a name="com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldRequest"></a>

### GetTreatmentFieldRequest
Request message for fetching a single treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field_id | [int64](#int64) | optional | The ID of the treatment field to fetch. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldResponse"></a>

### GetTreatmentFieldResponse
Response message with the fetched treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) |  | The treatment field with the provided ID. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsRequest"></a>

### ListTreatmentFieldsRequest
Request message for listing treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| plan_id | [int64](#int64) | optional | The ID of the plan for which to list treatment fields. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsResponse"></a>

### ListTreatmentFieldsResponse
Response message with the listed treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_fields | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) | repeated | The treatment fields matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldRequest"></a>

### UpdateTreatmentFieldRequest
Request message for updating an existing treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) | optional | The treatment field to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the treatment field to update. |






<a name="com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldResponse"></a>

### UpdateTreatmentFieldResponse
Response message with the updated treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_field | [TreatmentField](#com-empyreanmed-heracles-treatment_fields-v1-TreatmentField) |  | The updated treatment field. |





 

 

 


<a name="com-empyreanmed-heracles-treatment_fields-v1-TreatmentFieldService"></a>

### TreatmentFieldService
Performs operations on treatment fields.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListTreatmentFields | [ListTreatmentFieldsRequest](#com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsRequest) | [ListTreatmentFieldsResponse](#com-empyreanmed-heracles-treatment_fields-v1-ListTreatmentFieldsResponse) | Lists treatment fields for a given plan. |
| GetTreatmentField | [GetTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldRequest) | [GetTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-GetTreatmentFieldResponse) | Returns a single treatment field. |
| CreateTreatmentField | [CreateTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldRequest) | [CreateTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-CreateTreatmentFieldResponse) | Creates a new treatment field for a plan. |
| CreateBatchTreatmentFields | [CreateBatchTreatmentFieldsRequest](#com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsRequest) | [CreateBatchTreatmentFieldsResponse](#com-empyreanmed-heracles-treatment_fields-v1-CreateBatchTreatmentFieldsResponse) | Creates a batch of new treatment fields for a plan. |
| UpdateTreatmentField | [UpdateTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldRequest) | [UpdateTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-UpdateTreatmentFieldResponse) | Updates an existing treatment field. |
| DeleteTreatmentField | [DeleteTreatmentFieldRequest](#com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldRequest) | [DeleteTreatmentFieldResponse](#com-empyreanmed-heracles-treatment_fields-v1-DeleteTreatmentFieldResponse) | Deletes a treatment field. |

 



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

