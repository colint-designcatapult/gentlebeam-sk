# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/coil_configurations/v1/coil_configuration.proto](#com_empyreanmed_heracles_coil_configurations_v1_coil_configuration-proto)
    - [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration)
  
- [com/empyreanmed/heracles/coil_configurations/v1/coil_configuration_service.proto](#com_empyreanmed_heracles_coil_configurations_v1_coil_configuration_service-proto)
    - [CreateCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationRequest)
    - [CreateCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationResponse)
    - [DeleteCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationRequest)
    - [DeleteCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationResponse)
    - [GetCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationRequest)
    - [GetCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationResponse)
    - [ListCoilConfigurationsRequest](#com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsRequest)
    - [ListCoilConfigurationsResponse](#com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsResponse)
    - [UpdateCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationRequest)
    - [UpdateCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationResponse)
  
    - [CoilConfigurationService](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfigurationService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_coil_configurations_v1_coil_configuration-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/coil_configurations/v1/coil_configuration.proto



<a name="com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration"></a>

### CoilConfiguration
CoilConfiguration represents the coil configuration for an energy configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the coil configuration. |
| preset_configuration_id | [int64](#int64) | optional | Foreign key to the preset_configuration entity. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the coil configuration. |
| field_name | [com.empyreanmed.heracles.enums.v1.FIELDNAME](#com-empyreanmed-heracles-enums-v1-FIELDNAME) | optional | The field name of the coil configuration. |
| x_deflection_current | [float](#float) | optional | The X deflection current. |
| y_deflection_current | [float](#float) | optional | The Y deflection current. |
| focus_current | [float](#float) | optional | The focus current. |





 

 

 

 



<a name="com_empyreanmed_heracles_coil_configurations_v1_coil_configuration_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/coil_configurations/v1/coil_configuration_service.proto



<a name="com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationRequest"></a>

### CreateCoilConfigurationRequest
Request message for creating a new coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configuration | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) | optional | The coil configuration to be created. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationResponse"></a>

### CreateCoilConfigurationResponse
Response message with the created coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configuration | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) |  | The created coil configuration. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationRequest"></a>

### DeleteCoilConfigurationRequest
Request message for deleting a coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the coil configuration to be deleted. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationResponse"></a>

### DeleteCoilConfigurationResponse
An empty response message for `DeleteCoilConfiguration`.






<a name="com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationRequest"></a>

### GetCoilConfigurationRequest
Request message for fetching a single coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the coil configuration to be returned. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationResponse"></a>

### GetCoilConfigurationResponse
Response message for fetching a single coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configuration | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) |  | The coil configuration with the provided ID. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsRequest"></a>

### ListCoilConfigurationsRequest
Request message for listing coil configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of coil configurations to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListCoilConfigurations` call. |
| preset_configuration_id | [int64](#int64) | optional | The preset configuration ID to filter coil configurations by. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsResponse"></a>

### ListCoilConfigurationsResponse
Response message with the listed coil configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configurations | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) | repeated | The coil configurations matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationRequest"></a>

### UpdateCoilConfigurationRequest
Request message for updating an existing coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configuration | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) | optional | The coil configuration to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationResponse"></a>

### UpdateCoilConfigurationResponse
Response message with the updated coil configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| coil_configuration | [CoilConfiguration](#com-empyreanmed-heracles-coil_configurations-v1-CoilConfiguration) |  | The updated coil configuration. |





 

 

 


<a name="com-empyreanmed-heracles-coil_configurations-v1-CoilConfigurationService"></a>

### CoilConfigurationService
Performs CRUD operations on coil configurations.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListCoilConfigurations | [ListCoilConfigurationsRequest](#com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsRequest) | [ListCoilConfigurationsResponse](#com-empyreanmed-heracles-coil_configurations-v1-ListCoilConfigurationsResponse) | Lists coil configurations matching request parameters. |
| GetCoilConfiguration | [GetCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationRequest) | [GetCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-GetCoilConfigurationResponse) | Returns a single coil configuration. |
| CreateCoilConfiguration | [CreateCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationRequest) | [CreateCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-CreateCoilConfigurationResponse) | Creates a new coil configuration. |
| UpdateCoilConfiguration | [UpdateCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationRequest) | [UpdateCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-UpdateCoilConfigurationResponse) | Updates a single coil configuration. |
| DeleteCoilConfiguration | [DeleteCoilConfigurationRequest](#com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationRequest) | [DeleteCoilConfigurationResponse](#com-empyreanmed-heracles-coil_configurations-v1-DeleteCoilConfigurationResponse) | Deletes a single coil configuration. |

 



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

