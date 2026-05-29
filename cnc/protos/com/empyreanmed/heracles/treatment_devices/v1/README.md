# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/treatment_devices/v1/treatment_device.proto](#com_empyreanmed_heracles_treatment_devices_v1_treatment_device-proto)
    - [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice)
  
- [com/empyreanmed/heracles/treatment_devices/v1/treatment_devices_service.proto](#com_empyreanmed_heracles_treatment_devices_v1_treatment_devices_service-proto)
    - [CreateTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceRequest)
    - [CreateTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceResponse)
    - [DeleteTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceRequest)
    - [DeleteTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceResponse)
    - [GetTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceRequest)
    - [GetTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceResponse)
    - [ListTreatmentDevicesRequest](#com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesRequest)
    - [ListTreatmentDevicesResponse](#com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesResponse)
    - [UpdateTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceRequest)
    - [UpdateTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceResponse)
  
    - [TreatmentDevicesService](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevicesService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_treatment_devices_v1_treatment_device-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatment_devices/v1/treatment_device.proto



<a name="com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice"></a>

### TreatmentDevice
Represents a treatment device used in a simulation.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Device id, globally unique |
| simulation_id | [int64](#int64) | optional | Simulation id associated with the device |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Device creation date |
| device_name | [com.empyreanmed.heracles.enums.v1.DEVICETYPE](#com-empyreanmed-heracles-enums-v1-DEVICETYPE) | optional | Type of the treatment device |





 

 

 

 



<a name="com_empyreanmed_heracles_treatment_devices_v1_treatment_devices_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/treatment_devices/v1/treatment_devices_service.proto



<a name="com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceRequest"></a>

### CreateTreatmentDeviceRequest
Request message for creating a new treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) | optional | Details of the treatment device to create. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceResponse"></a>

### CreateTreatmentDeviceResponse
Response message with the created treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) | optional | The treatment device that was created. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceRequest"></a>

### DeleteTreatmentDeviceRequest
Request message for deleting a treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device_id | [int64](#int64) | optional | The ID of the treatment device to delete. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceResponse"></a>

### DeleteTreatmentDeviceResponse
An empty response message for `DeleteTreatmentDevice`.






<a name="com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceRequest"></a>

### GetTreatmentDeviceRequest
Request message for fetching a single treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device_id | [int64](#int64) | optional | The ID of the treatment device to fetch. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceResponse"></a>

### GetTreatmentDeviceResponse
Response message with the fetched treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) |  | The treatment device with the provided ID. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesRequest"></a>

### ListTreatmentDevicesRequest
Request message for listing treatment devices.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| simulation_id | [int64](#int64) | optional | The ID of the simulation for which to list treatment devices. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesResponse"></a>

### ListTreatmentDevicesResponse
Response message with the listed treatment devices.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_devices | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) | repeated | The treatment devices matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceRequest"></a>

### UpdateTreatmentDeviceRequest
Request message for updating an existing treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) | optional | The treatment device to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the treatment device to update. |






<a name="com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceResponse"></a>

### UpdateTreatmentDeviceResponse
Response message with the updated treatment device.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| treatment_device | [TreatmentDevice](#com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevice) |  | The updated treatment device. |





 

 

 


<a name="com-empyreanmed-heracles-treatment_devices-v1-TreatmentDevicesService"></a>

### TreatmentDevicesService
Performs operations on treatment devices.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListTreatmentDevices | [ListTreatmentDevicesRequest](#com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesRequest) | [ListTreatmentDevicesResponse](#com-empyreanmed-heracles-treatment_devices-v1-ListTreatmentDevicesResponse) | Lists treatment devices for a given clinic. |
| GetTreatmentDevice | [GetTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceRequest) | [GetTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-GetTreatmentDeviceResponse) | Returns a single treatment device. |
| CreateTreatmentDevice | [CreateTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceRequest) | [CreateTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-CreateTreatmentDeviceResponse) | Creates a new treatment device for a clinic. |
| UpdateTreatmentDevice | [UpdateTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceRequest) | [UpdateTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-UpdateTreatmentDeviceResponse) | Updates an existing treatment device. |
| DeleteTreatmentDevice | [DeleteTreatmentDeviceRequest](#com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceRequest) | [DeleteTreatmentDeviceResponse](#com-empyreanmed-heracles-treatment_devices-v1-DeleteTreatmentDeviceResponse) | Deletes a treatment device. |

 



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

