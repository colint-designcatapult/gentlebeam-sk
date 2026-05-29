# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/intensities/v1/intensity.proto](#com_empyreanmed_heracles_intensities_v1_intensity-proto)
    - [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity)
  
- [com/empyreanmed/heracles/intensities/v1/intensity_service.proto](#com_empyreanmed_heracles_intensities_v1_intensity_service-proto)
    - [CreateIntensityRequest](#com-empyreanmed-heracles-intensities-v1-CreateIntensityRequest)
    - [CreateIntensityResponse](#com-empyreanmed-heracles-intensities-v1-CreateIntensityResponse)
    - [DeleteIntensityRequest](#com-empyreanmed-heracles-intensities-v1-DeleteIntensityRequest)
    - [DeleteIntensityResponse](#com-empyreanmed-heracles-intensities-v1-DeleteIntensityResponse)
    - [GetIntensityRequest](#com-empyreanmed-heracles-intensities-v1-GetIntensityRequest)
    - [GetIntensityResponse](#com-empyreanmed-heracles-intensities-v1-GetIntensityResponse)
    - [ListIntensitiesRequest](#com-empyreanmed-heracles-intensities-v1-ListIntensitiesRequest)
    - [ListIntensitiesResponse](#com-empyreanmed-heracles-intensities-v1-ListIntensitiesResponse)
    - [UpdateIntensityRequest](#com-empyreanmed-heracles-intensities-v1-UpdateIntensityRequest)
    - [UpdateIntensityResponse](#com-empyreanmed-heracles-intensities-v1-UpdateIntensityResponse)
  
    - [IntensityService](#com-empyreanmed-heracles-intensities-v1-IntensityService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_intensities_v1_intensity-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/intensities/v1/intensity.proto



<a name="com-empyreanmed-heracles-intensities-v1-Intensity"></a>

### Intensity
The Intensity message represents the core data structure of an intensity entity.
It includes details such as associated patient information, diagnostic details, and metadata.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the intensity entity. This field is output-only and typically generated automatically. |
| qcsample_fields_id | [int64](#int64) | optional | Identifier linking this intensity to a specific QC sample field. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The timestamp representing when this intensity entry was created. |
| diode_name | [string](#string) | optional | The name of the diode associated with this intensity measurement. |
| intensity | [double](#double) | optional | The measured intensity value. |





 

 

 

 



<a name="com_empyreanmed_heracles_intensities_v1_intensity_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/intensities/v1/intensity_service.proto



<a name="com-empyreanmed-heracles-intensities-v1-CreateIntensityRequest"></a>

### CreateIntensityRequest
Request message for creating a new intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensity | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) | optional | The details of the intensity entity to create. |






<a name="com-empyreanmed-heracles-intensities-v1-CreateIntensityResponse"></a>

### CreateIntensityResponse
Response message containing the details of the newly created intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensity | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) |  | The intensity entity that was created. |






<a name="com-empyreanmed-heracles-intensities-v1-DeleteIntensityRequest"></a>

### DeleteIntensityRequest
Request message for deleting a specific intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The unique identifier of the intensity entity to delete. |






<a name="com-empyreanmed-heracles-intensities-v1-DeleteIntensityResponse"></a>

### DeleteIntensityResponse
Response message confirming the deletion of an intensity entity.






<a name="com-empyreanmed-heracles-intensities-v1-GetIntensityRequest"></a>

### GetIntensityRequest
Request message for retrieving a single intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The unique identifier of the intensity entity to retrieve. |






<a name="com-empyreanmed-heracles-intensities-v1-GetIntensityResponse"></a>

### GetIntensityResponse
Response message containing the details of a specific intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensity | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) |  | The intensity entity corresponding to the requested ID. |






<a name="com-empyreanmed-heracles-intensities-v1-ListIntensitiesRequest"></a>

### ListIntensitiesRequest
Request message for listing intensity entities.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| qcsample_fields_id | [int64](#int64) | optional | Filters the results by the ID of the associated qcsample_field. |






<a name="com-empyreanmed-heracles-intensities-v1-ListIntensitiesResponse"></a>

### ListIntensitiesResponse
Response message containing the list of intensity entities.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensities | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) | repeated | The list of intensity entities matching the filter criteria. |






<a name="com-empyreanmed-heracles-intensities-v1-UpdateIntensityRequest"></a>

### UpdateIntensityRequest
Request message for updating an existing intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensity | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) | optional | The updated details of the intensity entity. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | The field mask indicating the fields to update. |






<a name="com-empyreanmed-heracles-intensities-v1-UpdateIntensityResponse"></a>

### UpdateIntensityResponse
Response message containing the details of the updated intensity entity.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| intensity | [Intensity](#com-empyreanmed-heracles-intensities-v1-Intensity) |  | The intensity entity after the update. |





 

 

 


<a name="com-empyreanmed-heracles-intensities-v1-IntensityService"></a>

### IntensityService
The IntensityService defines the operations for managing intensity entities.
This includes creating, retrieving, updating, and deleting intensity records.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListIntensities | [ListIntensitiesRequest](#com-empyreanmed-heracles-intensities-v1-ListIntensitiesRequest) | [ListIntensitiesResponse](#com-empyreanmed-heracles-intensities-v1-ListIntensitiesResponse) | Retrieves a list of intensity entities based on provided filters. |
| GetIntensity | [GetIntensityRequest](#com-empyreanmed-heracles-intensities-v1-GetIntensityRequest) | [GetIntensityResponse](#com-empyreanmed-heracles-intensities-v1-GetIntensityResponse) | Fetches details of a specific intensity entity by ID. |
| CreateIntensity | [CreateIntensityRequest](#com-empyreanmed-heracles-intensities-v1-CreateIntensityRequest) | [CreateIntensityResponse](#com-empyreanmed-heracles-intensities-v1-CreateIntensityResponse) | Creates a new intensity entity and returns the created record. |
| UpdateIntensity | [UpdateIntensityRequest](#com-empyreanmed-heracles-intensities-v1-UpdateIntensityRequest) | [UpdateIntensityResponse](#com-empyreanmed-heracles-intensities-v1-UpdateIntensityResponse) | Updates an existing intensity entity based on the provided details and update mask. |
| DeleteIntensity | [DeleteIntensityRequest](#com-empyreanmed-heracles-intensities-v1-DeleteIntensityRequest) | [DeleteIntensityResponse](#com-empyreanmed-heracles-intensities-v1-DeleteIntensityResponse) | Deletes a specific intensity entity identified by its ID. |

 



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

