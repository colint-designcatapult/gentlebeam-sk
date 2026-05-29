# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/reference_fields/v1/reference_field.proto](#com_empyreanmed_heracles_reference_fields_v1_reference_field-proto)
    - [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField)
  
- [com/empyreanmed/heracles/reference_fields/v1/reference_field_service.proto](#com_empyreanmed_heracles_reference_fields_v1_reference_field_service-proto)
    - [CreateReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldRequest)
    - [CreateReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldResponse)
    - [DeleteReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldRequest)
    - [DeleteReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldResponse)
    - [GetReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldRequest)
    - [GetReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldResponse)
    - [ListReferenceFieldsRequest](#com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsRequest)
    - [ListReferenceFieldsResponse](#com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsResponse)
    - [UpdateReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldRequest)
    - [UpdateReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldResponse)
  
    - [ReferenceFieldService](#com-empyreanmed-heracles-reference_fields-v1-ReferenceFieldService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_reference_fields_v1_reference_field-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/reference_fields/v1/reference_field.proto



<a name="com-empyreanmed-heracles-reference_fields-v1-ReferenceField"></a>

### ReferenceField
ReferenceField represents the reference field for an energy configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the reference field. |
| preset_configuration_id | [int64](#int64) | optional | Foreign key to the preset_configuration entity. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the reference field. |
| magnetometer_type | [com.empyreanmed.heracles.enums.v1.MAGNETOMETERTYPE](#com-empyreanmed-heracles-enums-v1-MAGNETOMETERTYPE) | optional | The type of magnetometer. |
| rf11 | [float](#float) | optional | Reference field element RF11. |
| rf21 | [float](#float) | optional | Reference field element RF21. |
| rf31 | [float](#float) | optional | Reference field element RF31. |





 

 

 

 



<a name="com_empyreanmed_heracles_reference_fields_v1_reference_field_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/reference_fields/v1/reference_field_service.proto



<a name="com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldRequest"></a>

### CreateReferenceFieldRequest
Request message for creating a new reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_field | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) | optional | The reference field to be created. |






<a name="com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldResponse"></a>

### CreateReferenceFieldResponse
Response message with the created reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_field | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) |  | The created reference field. |






<a name="com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldRequest"></a>

### DeleteReferenceFieldRequest
Request message for deleting a reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the reference field to be deleted. |






<a name="com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldResponse"></a>

### DeleteReferenceFieldResponse
An empty response message for `DeleteReferenceField`.






<a name="com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldRequest"></a>

### GetReferenceFieldRequest
Request message for fetching a single reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the reference field to be returned. |






<a name="com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldResponse"></a>

### GetReferenceFieldResponse
Response message for fetching a single reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_field | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) |  | The reference field with the provided ID. |






<a name="com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsRequest"></a>

### ListReferenceFieldsRequest
Request message for listing reference fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of reference fields to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListReferenceFields` call. |
| preset_configuration_id | [int64](#int64) | optional | The preset configuration ID to filter reference fields by. |






<a name="com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsResponse"></a>

### ListReferenceFieldsResponse
Response message with the listed reference fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_fields | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) | repeated | The reference fields matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldRequest"></a>

### UpdateReferenceFieldRequest
Request message for updating an existing reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_field | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) | optional | The reference field to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldResponse"></a>

### UpdateReferenceFieldResponse
Response message with the updated reference field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| reference_field | [ReferenceField](#com-empyreanmed-heracles-reference_fields-v1-ReferenceField) |  | The updated reference field. |





 

 

 


<a name="com-empyreanmed-heracles-reference_fields-v1-ReferenceFieldService"></a>

### ReferenceFieldService
Performs CRUD operations on reference fields.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListReferenceFields | [ListReferenceFieldsRequest](#com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsRequest) | [ListReferenceFieldsResponse](#com-empyreanmed-heracles-reference_fields-v1-ListReferenceFieldsResponse) | Lists reference fields matching request parameters. |
| GetReferenceField | [GetReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldRequest) | [GetReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-GetReferenceFieldResponse) | Returns a single reference field. |
| CreateReferenceField | [CreateReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldRequest) | [CreateReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-CreateReferenceFieldResponse) | Creates a new reference field. |
| UpdateReferenceField | [UpdateReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldRequest) | [UpdateReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-UpdateReferenceFieldResponse) | Updates a single reference field. |
| DeleteReferenceField | [DeleteReferenceFieldRequest](#com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldRequest) | [DeleteReferenceFieldResponse](#com-empyreanmed-heracles-reference_fields-v1-DeleteReferenceFieldResponse) | Deletes a single reference field. |

 



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

