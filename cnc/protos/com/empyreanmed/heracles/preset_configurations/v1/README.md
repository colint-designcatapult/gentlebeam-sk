# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/preset_configurations/v1/preset_configuration.proto](#com_empyreanmed_heracles_preset_configurations_v1_preset_configuration-proto)
    - [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration)
  
- [com/empyreanmed/heracles/preset_configurations/v1/preset_configuration_service.proto](#com_empyreanmed_heracles_preset_configurations_v1_preset_configuration_service-proto)
    - [ApprovePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationRequest)
    - [ApprovePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationResponse)
    - [CreatePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationRequest)
    - [CreatePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationResponse)
    - [DeletePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationRequest)
    - [DeletePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationResponse)
    - [GetPresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationRequest)
    - [GetPresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationResponse)
    - [ListPresetConfigurationsRequest](#com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsRequest)
    - [ListPresetConfigurationsResponse](#com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsResponse)
    - [UpdatePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationRequest)
    - [UpdatePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationResponse)
  
    - [PresetConfigurationService](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfigurationService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_preset_configurations_v1_preset_configuration-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/preset_configurations/v1/preset_configuration.proto



<a name="com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration"></a>

### PresetConfiguration
PresetConfiguration represents a preset configuration for a target.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the preset configuration. |
| collimator_configuration_id | [int64](#int64) | optional | Foreign key to the collimator configuration entity. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the preset configuration. |
| preset_name | [string](#string) | optional | Name of the preset configuration. |
| is_default | [bool](#bool) | optional | Indicates if this is the default preset configuration. |
| is_active | [bool](#bool) | optional | Indicates if the preset configuration is active. |
| approved_by | [string](#string) | optional | The user ID of the approver this item. |





 

 

 

 



<a name="com_empyreanmed_heracles_preset_configurations_v1_preset_configuration_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/preset_configurations/v1/preset_configuration_service.proto



<a name="com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationRequest"></a>

### ApprovePresetConfigurationRequest
Request message for approving a preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| username | [string](#string) |  | The username of the user approving the preset configuration. |
| password | [string](#string) |  | The password of the user approving the preset configuration. |
| preset_configuration_id | [int64](#int64) |  | The ID of the preset configuration to approve. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationResponse"></a>

### ApprovePresetConfigurationResponse
Response message for approving a preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| approved_preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) |  | The approved preset configuration. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationRequest"></a>

### CreatePresetConfigurationRequest
Request message for creating a new preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) | optional | The preset configuration to be created. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationResponse"></a>

### CreatePresetConfigurationResponse
Response message with the created preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) | optional | The preset configuration that was created. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationRequest"></a>

### DeletePresetConfigurationRequest
Request message for deleting a preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the preset configuration to be deleted. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationResponse"></a>

### DeletePresetConfigurationResponse
An empty response message for `DeletePresetConfiguration`.






<a name="com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationRequest"></a>

### GetPresetConfigurationRequest
Request message for fetching a single preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the preset configuration to be returned. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationResponse"></a>

### GetPresetConfigurationResponse
Response message for fetching a single preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) |  | The preset configuration with the ID provided in the request. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsRequest"></a>

### ListPresetConfigurationsRequest
Request message for listing preset configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of preset configurations to return. The service may return fewer than this value. If unset or zero, all preset configurations will be returned. |
| page_token | [string](#string) | optional | A page token, received from a previous `ListPresetConfigurations` call. Provide this to retrieve the subsequent page.

When paginating, all other parameters provided to `ListPresetConfigurations` must match the call that provided the page token. |
| collimator_configuration_id | [int64](#int64) | optional | The collimator configuration ID to filter preset configurations by. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsResponse"></a>

### ListPresetConfigurationsResponse
Response message with the listed preset configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configurations | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) | repeated | The preset configurations matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationRequest"></a>

### UpdatePresetConfigurationRequest
Request message for updating an existing preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) | optional | The preset configuration to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the preset configuration to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationResponse"></a>

### UpdatePresetConfigurationResponse
Response message with the updated preset configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| preset_configuration | [PresetConfiguration](#com-empyreanmed-heracles-preset_configurations-v1-PresetConfiguration) |  | The updated preset configuration. |





 

 

 


<a name="com-empyreanmed-heracles-preset_configurations-v1-PresetConfigurationService"></a>

### PresetConfigurationService
Performs CRUD operations on preset configurations.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPresetConfigurations | [ListPresetConfigurationsRequest](#com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsRequest) | [ListPresetConfigurationsResponse](#com-empyreanmed-heracles-preset_configurations-v1-ListPresetConfigurationsResponse) | Lists preset configurations matching request parameters. |
| GetPresetConfiguration | [GetPresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationRequest) | [GetPresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-GetPresetConfigurationResponse) | Returns a single preset configuration. |
| CreatePresetConfiguration | [CreatePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationRequest) | [CreatePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-CreatePresetConfigurationResponse) | Creates a new preset configuration. |
| UpdatePresetConfiguration | [UpdatePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationRequest) | [UpdatePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-UpdatePresetConfigurationResponse) | Updates a single preset configuration. |
| DeletePresetConfiguration | [DeletePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationRequest) | [DeletePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-DeletePresetConfigurationResponse) | Deletes a single preset configuration. |
| ApprovePresetConfiguration | [ApprovePresetConfigurationRequest](#com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationRequest) | [ApprovePresetConfigurationResponse](#com-empyreanmed-heracles-preset_configurations-v1-ApprovePresetConfigurationResponse) | Approves a preset configuration. |

 



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

