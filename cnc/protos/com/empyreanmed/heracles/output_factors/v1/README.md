# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/output_factors/v1/output_factor.proto](#com_empyreanmed_heracles_output_factors_v1_output_factor-proto)
    - [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor)
  
- [com/empyreanmed/heracles/output_factors/v1/output_factor_service.proto](#com_empyreanmed_heracles_output_factors_v1_output_factor_service-proto)
    - [CreateOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorRequest)
    - [CreateOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorResponse)
    - [DeleteOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorRequest)
    - [DeleteOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorResponse)
    - [GetOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-GetOutputFactorRequest)
    - [GetOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-GetOutputFactorResponse)
    - [ListOutputFactorsRequest](#com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsRequest)
    - [ListOutputFactorsResponse](#com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsResponse)
    - [UpdateOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorRequest)
    - [UpdateOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorResponse)
  
    - [OutputFactorService](#com-empyreanmed-heracles-output_factors-v1-OutputFactorService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_output_factors_v1_output_factor-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/output_factors/v1/output_factor.proto



<a name="com-empyreanmed-heracles-output_factors-v1-OutputFactor"></a>

### OutputFactor
OutputFactor represents an output factor entity in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the output factor. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the output factor. |
| preset_configuration_id | [int64](#int64) | optional | The ID of the related preset configuration. |
| field_name | [com.empyreanmed.heracles.enums.v1.FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME) | optional | The name of the field associated with this output factor. |
| factor | [float](#float) | optional | The factor value for this output factor. |





 

 

 

 



<a name="com_empyreanmed_heracles_output_factors_v1_output_factor_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/output_factors/v1/output_factor_service.proto



<a name="com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorRequest"></a>

### CreateOutputFactorRequest
Request message for creating a new OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factor | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) | optional | The OutputFactor to be created. |






<a name="com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorResponse"></a>

### CreateOutputFactorResponse
Response message with the created OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factor | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) | optional | The OutputFactor that was created. |






<a name="com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorRequest"></a>

### DeleteOutputFactorRequest
Request message for deleting an OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the OutputFactor to be deleted. |






<a name="com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorResponse"></a>

### DeleteOutputFactorResponse
An empty response message for `DeleteOutputFactor`.






<a name="com-empyreanmed-heracles-output_factors-v1-GetOutputFactorRequest"></a>

### GetOutputFactorRequest
Request message for fetching a single OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the OutputFactor to be returned. |






<a name="com-empyreanmed-heracles-output_factors-v1-GetOutputFactorResponse"></a>

### GetOutputFactorResponse
Response message for fetching a single OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factor | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) |  | The OutputFactor with the ID provided in the request. |






<a name="com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsRequest"></a>

### ListOutputFactorsRequest
Request message for listing OutputFactors.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of OutputFactors to return. The service may return fewer than this value. If unset or zero, all OutputFactors will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListOutputFactors` call. Provide this to retrieve the subsequent page.

When paginating, all other parameters provided to `ListOutputFactors` must match the call that provided the page token. |
| preset_configuration_id | [int64](#int64) | optional | The preset configuration ID to filter OutputFactors by. |






<a name="com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsResponse"></a>

### ListOutputFactorsResponse
Response message with the listed OutputFactors.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factors | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) | repeated | The OutputFactors matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorRequest"></a>

### UpdateOutputFactorRequest
Request message for updating an existing OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factor | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) | optional | The OutputFactor to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the OutputFactor to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorResponse"></a>

### UpdateOutputFactorResponse
Response message with the updated OutputFactor.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| output_factor | [OutputFactor](#com-empyreanmed-heracles-output_factors-v1-OutputFactor) |  | The updated OutputFactor. |





 

 

 


<a name="com-empyreanmed-heracles-output_factors-v1-OutputFactorService"></a>

### OutputFactorService
Performs CRUD operations on OutputFactors.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListOutputFactors | [ListOutputFactorsRequest](#com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsRequest) | [ListOutputFactorsResponse](#com-empyreanmed-heracles-output_factors-v1-ListOutputFactorsResponse) | Lists OutputFactors matching request parameters. |
| GetOutputFactor | [GetOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-GetOutputFactorRequest) | [GetOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-GetOutputFactorResponse) | Returns a single OutputFactor. |
| CreateOutputFactor | [CreateOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorRequest) | [CreateOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-CreateOutputFactorResponse) | Creates a new OutputFactor. |
| UpdateOutputFactor | [UpdateOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorRequest) | [UpdateOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-UpdateOutputFactorResponse) | Updates a single OutputFactor. |
| DeleteOutputFactor | [DeleteOutputFactorRequest](#com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorRequest) | [DeleteOutputFactorResponse](#com-empyreanmed-heracles-output_factors-v1-DeleteOutputFactorResponse) | Deletes a single OutputFactor. |

 



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

