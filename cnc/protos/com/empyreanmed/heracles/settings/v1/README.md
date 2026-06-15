# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/settings/v1/settings.proto](#com_empyreanmed_heracles_settings_v1_settings-proto)
    - [Settings](#com-empyreanmed-heracles-settings-v1-Settings)
  
- [com/empyreanmed/heracles/settings/v1/settings_service.proto](#com_empyreanmed_heracles_settings_v1_settings_service-proto)
    - [GetSettingsRequest](#com-empyreanmed-heracles-settings-v1-GetSettingsRequest)
    - [GetSettingsResponse](#com-empyreanmed-heracles-settings-v1-GetSettingsResponse)
    - [UpdateSettingsRequest](#com-empyreanmed-heracles-settings-v1-UpdateSettingsRequest)
    - [UpdateSettingsResponse](#com-empyreanmed-heracles-settings-v1-UpdateSettingsResponse)
  
    - [SettingsService](#com-empyreanmed-heracles-settings-v1-SettingsService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_settings_v1_settings-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/settings/v1/settings.proto



<a name="com-empyreanmed-heracles-settings-v1-Settings"></a>

### Settings
Settings represents configuration settings.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Settings id, globally unique |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The date the settings were created |
| device_serial | [string](#string) | optional | The serial number of the device |
| record_and_verify_ip | [string](#string) | optional | Endpoint for record and verify IP |
| record_and_verify_port | [string](#string) | optional | Endpoint for record and verify port |
| database_ip | [string](#string) | optional | Endpoint for database IP |
| database_port | [string](#string) | optional | Endpoint for database port |
| imaging_headcam_ip | [string](#string) | optional | Endpoint for imaging head camera IP |
| imaging_headcam_port | [string](#string) | optional | Endpoint for imaging head camera port |
| treatment_headcam_ip | [string](#string) | optional | Endpoint for treatment head camera IP |
| treatment_headcam_port | [string](#string) | optional | Endpoint for treatment head camera port |
| robotcam_ip | [string](#string) | optional | Endpoint for robot camera IP |
| robotcam_port | [string](#string) | optional | Endpoint for robot camera port |
| gcb_telemetry_ip | [string](#string) | optional | Endpoint for GCB telemetry IP |
| gcb_telemetry_port | [string](#string) | optional | Endpoint for GCB telemetry port |
| gcb_commands_ip | [string](#string) | optional | Endpoint for GCB commands IP |
| gcb_commands_port | [string](#string) | optional | Endpoint for GCB commands port |
| robotic_ros_ip | [string](#string) | optional | Endpoint for robotic ROS IP |
| robotic_ros_port | [string](#string) | optional | Endpoint for robotic ROS port |
| data_acquisition_ip | [string](#string) | optional | Endpoint for data acquisition IP |
| data_acquisition_port | [string](#string) | optional | Endpoint for data acquisition port |
| dc_data_reconstruction_ip | [string](#string) | optional | Endpoint for DC data reconstruction IP |
| dc_data_reconstruction_port | [string](#string) | optional | Endpoint for DC data reconstruction port |
| dc_data_progress_websocket_ip | [string](#string) | optional | Endpoint for DC data progress websocket IP |
| dc_data_progress_websocket_port | [string](#string) | optional | Endpoint for DC data progress websocket port |
| dc_data_reconstruction_z_mq_ip | [string](#string) | optional | Endpoint for DC data reconstruction Z MQ IP |
| dc_data_reconstruction_z_mq_port | [string](#string) | optional | Endpoint for DC data reconstruction Z MQ port |
| dc_database_ip | [string](#string) | optional | Endpoint for DC database IP |
| dc_database_port | [string](#string) | optional | Endpoint for DC database port |





 

 

 

 



<a name="com_empyreanmed_heracles_settings_v1_settings_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/settings/v1/settings_service.proto



<a name="com-empyreanmed-heracles-settings-v1-GetSettingsRequest"></a>

### GetSettingsRequest
Request message for retrieving the current settings.






<a name="com-empyreanmed-heracles-settings-v1-GetSettingsResponse"></a>

### GetSettingsResponse
Response message with the current settings.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| settings | [Settings](#com-empyreanmed-heracles-settings-v1-Settings) |  | The current settings. |






<a name="com-empyreanmed-heracles-settings-v1-UpdateSettingsRequest"></a>

### UpdateSettingsRequest
Request message for updating the settings.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| settings | [Settings](#com-empyreanmed-heracles-settings-v1-Settings) | optional | The settings to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the settings to update. |






<a name="com-empyreanmed-heracles-settings-v1-UpdateSettingsResponse"></a>

### UpdateSettingsResponse
Response message with the updated settings.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| settings | [Settings](#com-empyreanmed-heracles-settings-v1-Settings) |  | The updated settings. |





 

 

 


<a name="com-empyreanmed-heracles-settings-v1-SettingsService"></a>

### SettingsService
Performs operations on Settings.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| GetSettings | [GetSettingsRequest](#com-empyreanmed-heracles-settings-v1-GetSettingsRequest) | [GetSettingsResponse](#com-empyreanmed-heracles-settings-v1-GetSettingsResponse) | Retrieves the current settings. |
| UpdateSettings | [UpdateSettingsRequest](#com-empyreanmed-heracles-settings-v1-UpdateSettingsRequest) | [UpdateSettingsResponse](#com-empyreanmed-heracles-settings-v1-UpdateSettingsResponse) | Updates the settings. |

 



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

