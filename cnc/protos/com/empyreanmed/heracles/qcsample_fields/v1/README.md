# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/qcsample_fields/v1/qcsample_field.proto](#com_empyreanmed_heracles_qcsample_fields_v1_qcsample_field-proto)
    - [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField)
  
- [com/empyreanmed/heracles/qcsample_fields/v1/qcsample_field_service.proto](#com_empyreanmed_heracles_qcsample_fields_v1_qcsample_field_service-proto)
    - [CreateQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldRequest)
    - [CreateQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldResponse)
    - [DeleteQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldRequest)
    - [DeleteQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldResponse)
    - [GetQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldRequest)
    - [GetQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldResponse)
    - [ListQCSampleFieldsRequest](#com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsRequest)
    - [ListQCSampleFieldsResponse](#com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsResponse)
    - [UpdateQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldRequest)
    - [UpdateQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldResponse)
  
    - [QCSampleFieldService](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleFieldService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_qcsample_fields_v1_qcsample_field-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/qcsample_fields/v1/qcsample_field.proto



<a name="com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField"></a>

### QCSampleField
QCSampleField represents a field in a quality control sample.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | QCSampleField id, globally unique |
| qcsample_id | [int64](#int64) | optional | The associated QCSample id |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The date the field was created |
| field | [com.empyreanmed.heracles.enums.v1.FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME) | optional | The field name |





 

 

 

 



<a name="com_empyreanmed_heracles_qcsample_fields_v1_qcsample_field_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/qcsample_fields/v1/qcsample_field_service.proto



<a name="com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldRequest"></a>

### CreateQCSampleFieldRequest
Request message for creating a new quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) | optional | Details of the quality control sample field to create. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldResponse"></a>

### CreateQCSampleFieldResponse
Response message with the created quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) |  | The quality control sample field that was created. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldRequest"></a>

### DeleteQCSampleFieldRequest
Request message for deleting a quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield_id | [int64](#int64) | optional | The ID of the quality control sample field to delete. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldResponse"></a>

### DeleteQCSampleFieldResponse
An empty response message for `DeleteQCSampleField`.






<a name="com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldRequest"></a>

### GetQCSampleFieldRequest
Request message for fetching a single quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield_id | [int64](#int64) | optional | The ID of the quality control sample field to fetch. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldResponse"></a>

### GetQCSampleFieldResponse
Response message with the fetched quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) |  | The quality control sample field with the provided ID. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsRequest"></a>

### ListQCSampleFieldsRequest
Request message for listing quality control sample fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample_id | [int64](#int64) | optional | The ID of the quality control sample for which to list fields. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsResponse"></a>

### ListQCSampleFieldsResponse
Response message with the listed quality control sample fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefields | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) | repeated | The quality control sample fields matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldRequest"></a>

### UpdateQCSampleFieldRequest
Request message for updating an existing quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) | optional | The quality control sample field to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the quality control sample field to update. |






<a name="com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldResponse"></a>

### UpdateQCSampleFieldResponse
Response message with the updated quality control sample field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsamplefield | [QCSampleField](#com-empyreanmed-heracles-qcsample_fields-v1-QCSampleField) |  | The updated quality control sample field. |





 

 

 


<a name="com-empyreanmed-heracles-qcsample_fields-v1-QCSampleFieldService"></a>

### QCSampleFieldService
Performs operations on QCSampleFields.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListQCSampleFields | [ListQCSampleFieldsRequest](#com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsRequest) | [ListQCSampleFieldsResponse](#com-empyreanmed-heracles-qcsample_fields-v1-ListQCSampleFieldsResponse) | Lists fields for a given quality control sample. |
| GetQCSampleField | [GetQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldRequest) | [GetQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-GetQCSampleFieldResponse) | Returns a single quality control sample field. |
| CreateQCSampleField | [CreateQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldRequest) | [CreateQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-CreateQCSampleFieldResponse) | Creates a new quality control sample field. |
| UpdateQCSampleField | [UpdateQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldRequest) | [UpdateQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-UpdateQCSampleFieldResponse) | Updates an existing quality control sample field. |
| DeleteQCSampleField | [DeleteQCSampleFieldRequest](#com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldRequest) | [DeleteQCSampleFieldResponse](#com-empyreanmed-heracles-qcsample_fields-v1-DeleteQCSampleFieldResponse) | Deletes a quality control sample field. |

 



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

