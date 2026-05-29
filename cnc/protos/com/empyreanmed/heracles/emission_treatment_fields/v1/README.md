# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/emission_treatment_fields/v1/emission_treatment_field.proto](#com_empyreanmed_heracles_emission_treatment_fields_v1_emission_treatment_field-proto)
    - [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField)
  
- [com/empyreanmed/heracles/emission_treatment_fields/v1/emission_treatment_field_service.proto](#com_empyreanmed_heracles_emission_treatment_fields_v1_emission_treatment_field_service-proto)
    - [CreateEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldRequest)
    - [CreateEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldResponse)
    - [DeleteEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldRequest)
    - [DeleteEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldResponse)
    - [GetEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldRequest)
    - [GetEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldResponse)
    - [ListEmissionTreatmentFieldsRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsRequest)
    - [ListEmissionTreatmentFieldsResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsResponse)
    - [UpdateEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldRequest)
    - [UpdateEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldResponse)
  
    - [EmissionTreatmentFieldService](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentFieldService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_emission_treatment_fields_v1_emission_treatment_field-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/emission_treatment_fields/v1/emission_treatment_field.proto



<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField"></a>

### EmissionTreatmentField
Represents an emission treatment field associated with an actual treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Emission treatment field id, globally unique |
| actual_treatment_field_id | [int64](#int64) | optional | Actual treatment field id associated with the emission treatment field |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Emission treatment field creation date |
| actual_dwell_time | [double](#double) | optional | Actual dwell time of the emission treatment field |





 

 

 

 



<a name="com_empyreanmed_heracles_emission_treatment_fields_v1_emission_treatment_field_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/emission_treatment_fields/v1/emission_treatment_field_service.proto



<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldRequest"></a>

### CreateEmissionTreatmentFieldRequest
Request message for creating a new emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) | optional | Details of the emission treatment field to create. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldResponse"></a>

### CreateEmissionTreatmentFieldResponse
Response message with the created emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) |  | The emission treatment field that was created. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldRequest"></a>

### DeleteEmissionTreatmentFieldRequest
Request message for deleting an emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field_id | [int64](#int64) | optional | The ID of the emission treatment field to delete. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldResponse"></a>

### DeleteEmissionTreatmentFieldResponse
An empty response message for `DeleteEmissionTreatmentField`.






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldRequest"></a>

### GetEmissionTreatmentFieldRequest
Request message for fetching a single emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field_id | [int64](#int64) | optional | The ID of the emission treatment field to fetch. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldResponse"></a>

### GetEmissionTreatmentFieldResponse
Response message with the fetched emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) |  | The emission treatment field with the provided ID. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsRequest"></a>

### ListEmissionTreatmentFieldsRequest
Request message for listing emission treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| actual_treatment_field_id | [int64](#int64) | optional | The ID of the ActualTreatmentField_id for which to list emission treatment fields. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsResponse"></a>

### ListEmissionTreatmentFieldsResponse
Response message with the listed emission treatment fields.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_fields | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) | repeated | The emission treatment fields matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldRequest"></a>

### UpdateEmissionTreatmentFieldRequest
Request message for updating an existing emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) | optional | The emission treatment field to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the emission treatment field to update. |






<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldResponse"></a>

### UpdateEmissionTreatmentFieldResponse
Response message with the updated emission treatment field.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| emission_treatment_field | [EmissionTreatmentField](#com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentField) |  | The updated emission treatment field. |





 

 

 


<a name="com-empyreanmed-heracles-emission_treatment_fields-v1-EmissionTreatmentFieldService"></a>

### EmissionTreatmentFieldService
Performs operations on emission treatment fields.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListEmissionTreatmentFields | [ListEmissionTreatmentFieldsRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsRequest) | [ListEmissionTreatmentFieldsResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-ListEmissionTreatmentFieldsResponse) | Lists emission treatment fields for a given plan. |
| GetEmissionTreatmentField | [GetEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldRequest) | [GetEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-GetEmissionTreatmentFieldResponse) | Returns a single emission treatment field. |
| CreateEmissionTreatmentField | [CreateEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldRequest) | [CreateEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-CreateEmissionTreatmentFieldResponse) | Creates a new emission treatment field for a plan. |
| UpdateEmissionTreatmentField | [UpdateEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldRequest) | [UpdateEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-UpdateEmissionTreatmentFieldResponse) | Updates an existing emission treatment field. |
| DeleteEmissionTreatmentField | [DeleteEmissionTreatmentFieldRequest](#com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldRequest) | [DeleteEmissionTreatmentFieldResponse](#com-empyreanmed-heracles-emission_treatment_fields-v1-DeleteEmissionTreatmentFieldResponse) | Deletes an emission treatment field. |

 



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

