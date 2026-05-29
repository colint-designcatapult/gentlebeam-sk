# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/heater_current_configs/v1/heater_current_config.proto](#com_empyreanmed_heracles_heater_current_configs_v1_heater_current_config-proto)
    - [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig)
  
- [com/empyreanmed/heracles/heater_current_configs/v1/heater_current_config_service.proto](#com_empyreanmed_heracles_heater_current_configs_v1_heater_current_config_service-proto)
    - [CreateHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigRequest)
    - [CreateHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigResponse)
    - [DeleteHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigRequest)
    - [DeleteHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigResponse)
    - [GetHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigRequest)
    - [GetHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigResponse)
    - [ListHeaterCurrentConfigsRequest](#com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsRequest)
    - [ListHeaterCurrentConfigsResponse](#com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsResponse)
    - [UpdateHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigRequest)
    - [UpdateHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigResponse)
  
    - [HeaterCurrentConfigService](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfigService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_heater_current_configs_v1_heater_current_config-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/heater_current_configs/v1/heater_current_config.proto



<a name="com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig"></a>

### HeaterCurrentConfig
HeaterCurrentConfig represents the heater current configuration for an energy configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the heater current configuration. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the heater current configuration. |
| preset_configuration_id | [int64](#int64) | optional | Foreign key to the preset_configuration entity. |
| heater_current | [float](#float) | optional | The heater current value. |





 

 

 

 



<a name="com_empyreanmed_heracles_heater_current_configs_v1_heater_current_config_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/heater_current_configs/v1/heater_current_config_service.proto



<a name="com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigRequest"></a>

### CreateHeaterCurrentConfigRequest
Request message for creating a new heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_config | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) | optional | The heater current configuration to be created. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigResponse"></a>

### CreateHeaterCurrentConfigResponse
Response message with the created heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_config | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) |  | The created heater current configuration. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigRequest"></a>

### DeleteHeaterCurrentConfigRequest
Request message for deleting a heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the heater current configuration to be deleted. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigResponse"></a>

### DeleteHeaterCurrentConfigResponse
An empty response message for `DeleteHeaterCurrentConfig`.






<a name="com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigRequest"></a>

### GetHeaterCurrentConfigRequest
Request message for fetching a single heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the heater current configuration to be returned. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigResponse"></a>

### GetHeaterCurrentConfigResponse
Response message for fetching a single heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_config | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) |  | The heater current configuration with the provided ID. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsRequest"></a>

### ListHeaterCurrentConfigsRequest
Request message for listing heater current configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of heater current configurations to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListHeaterCurrentConfigs` call. |
| preset_configuration_id | [int64](#int64) | optional | The preset configuration ID to filter heater current configurations by. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsResponse"></a>

### ListHeaterCurrentConfigsResponse
Response message with the listed heater current configurations.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_configs | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) | repeated | The heater current configurations matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigRequest"></a>

### UpdateHeaterCurrentConfigRequest
Request message for updating an existing heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_config | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) | optional | The heater current configuration to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigResponse"></a>

### UpdateHeaterCurrentConfigResponse
Response message with the updated heater current configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| heater_current_config | [HeaterCurrentConfig](#com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfig) |  | The updated heater current configuration. |





 

 

 


<a name="com-empyreanmed-heracles-heater_current_configs-v1-HeaterCurrentConfigService"></a>

### HeaterCurrentConfigService
Performs CRUD operations on heater current configurations.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListHeaterCurrentConfigs | [ListHeaterCurrentConfigsRequest](#com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsRequest) | [ListHeaterCurrentConfigsResponse](#com-empyreanmed-heracles-heater_current_configs-v1-ListHeaterCurrentConfigsResponse) | Lists heater current configurations matching request parameters. |
| GetHeaterCurrentConfig | [GetHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigRequest) | [GetHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-GetHeaterCurrentConfigResponse) | Returns a single heater current configuration. |
| CreateHeaterCurrentConfig | [CreateHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigRequest) | [CreateHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-CreateHeaterCurrentConfigResponse) | Creates a new heater current configuration. |
| UpdateHeaterCurrentConfig | [UpdateHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigRequest) | [UpdateHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-UpdateHeaterCurrentConfigResponse) | Updates a single heater current configuration. |
| DeleteHeaterCurrentConfig | [DeleteHeaterCurrentConfigRequest](#com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigRequest) | [DeleteHeaterCurrentConfigResponse](#com-empyreanmed-heracles-heater_current_configs-v1-DeleteHeaterCurrentConfigResponse) | Deletes a single heater current configuration. |

 



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

