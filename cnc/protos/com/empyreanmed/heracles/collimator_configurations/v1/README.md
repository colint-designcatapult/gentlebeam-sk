# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/collimator_configurations/v1/collimator_configuration.proto](#com_empyreanmed_heracles_collimator_configurations_v1_collimator_configuration-proto)
    - [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration)
  
- [com/empyreanmed/heracles/collimator_configurations/v1/collimator_configuration_service.proto](#com_empyreanmed_heracles_collimator_configurations_v1_collimator_configuration_service-proto)
    - [CreateCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationRequest)
    - [CreateCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationResponse)
    - [DeleteCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationRequest)
    - [DeleteCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationResponse)
    - [GetCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationRequest)
    - [GetCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationResponse)
    - [ListCollimatorConfigurationsRequest](#com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsRequest)
    - [ListCollimatorConfigurationsResponse](#com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsResponse)
    - [SearchCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationRequest)
    - [SearchCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationResponse)
    - [UpdateCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationRequest)
    - [UpdateCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationResponse)
  
    - [CollimatorConfigurationService](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfigurationService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_collimator_configurations_v1_collimator_configuration-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/collimator_configurations/v1/collimator_configuration.proto



<a name="com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration"></a>

### CollimatorConfiguration
Represents a collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the configuration |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Date when the configuration was created |
| type | [com.empyreanmed.heracles.enums.v1.TARGETTYPE](#com-empyreanmed-heracles-enums-v1-TARGETTYPE) | optional | Type of the collimator |
| energy | [com.empyreanmed.heracles.enums.v1.ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY) | optional | Energy level of the collimator |
| power | [int32](#int32) | optional | Power level |
| ssd | [com.empyreanmed.heracles.enums.v1.SSDTYPE](#com-empyreanmed-heracles-enums-v1-SSDTYPE) | optional | Source-to-skin distance |
| referenced_dose_rate | [float](#float) | optional | Referenced dose rate |





 

 

 

 



<a name="com_empyreanmed_heracles_collimator_configurations_v1_collimator_configuration_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/collimator_configurations/v1/collimator_configuration_service.proto



<a name="com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationRequest"></a>

### CreateCollimatorConfigurationRequest
Request message for creating a new collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | Collimator configuration to create |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationResponse"></a>

### CreateCollimatorConfigurationResponse
Response message for creating a new collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | The created collimator configuration |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationRequest"></a>

### DeleteCollimatorConfigurationRequest
Request message for deleting a collimator configuration by ID


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | ID of the collimator configuration to delete |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationResponse"></a>

### DeleteCollimatorConfigurationResponse
Response message for deleting a collimator configuration






<a name="com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationRequest"></a>

### GetCollimatorConfigurationRequest
Request message for getting a single collimator configuration by ID


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) |  | ID of the requested configuration |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationResponse"></a>

### GetCollimatorConfigurationResponse
Response message for getting a single collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | The requested collimator configuration |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsRequest"></a>

### ListCollimatorConfigurationsRequest
Request message for listing collimator configurations






<a name="com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsResponse"></a>

### ListCollimatorConfigurationsResponse
Response message for listing collimator configurations


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configurations | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) | repeated | List of collimator configurations |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationRequest"></a>

### SearchCollimatorConfigurationRequest
Request message for searching collimator configurations


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| target_type | [com.empyreanmed.heracles.enums.v1.TARGETTYPE](#com-empyreanmed-heracles-enums-v1-TARGETTYPE) | optional | Filter by target type |
| energy | [com.empyreanmed.heracles.enums.v1.ENERGY](#com-empyreanmed-heracles-enums-v1-ENERGY) | optional | Filter by energy |
| ssd | [com.empyreanmed.heracles.enums.v1.SSDTYPE](#com-empyreanmed-heracles-enums-v1-SSDTYPE) | optional | Filter by SSD type |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationResponse"></a>

### SearchCollimatorConfigurationResponse
Response message for searching collimator configurations


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configurations | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | List of collimator configurations matching the search criteria |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationRequest"></a>

### UpdateCollimatorConfigurationRequest
Request message for updating an existing collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | Collimator configuration with updates |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationResponse"></a>

### UpdateCollimatorConfigurationResponse
Response message for updating an existing collimator configuration


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| collimator_configuration | [CollimatorConfiguration](#com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfiguration) |  | The updated collimator configuration |





 

 

 


<a name="com-empyreanmed-heracles-collimator_configurations-v1-CollimatorConfigurationService"></a>

### CollimatorConfigurationService
Service definition for CollimatorConfiguration

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListCollimatorConfigurations | [ListCollimatorConfigurationsRequest](#com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsRequest) | [ListCollimatorConfigurationsResponse](#com-empyreanmed-heracles-collimator_configurations-v1-ListCollimatorConfigurationsResponse) | RPC method to list all collimator configurations |
| GetCollimatorConfiguration | [GetCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationRequest) | [GetCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-GetCollimatorConfigurationResponse) | RPC method to get a single collimator configuration by ID |
| CreateCollimatorConfiguration | [CreateCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationRequest) | [CreateCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-CreateCollimatorConfigurationResponse) | RPC method to create a new collimator configuration |
| UpdateCollimatorConfiguration | [UpdateCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationRequest) | [UpdateCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-UpdateCollimatorConfigurationResponse) | RPC method to update an existing collimator configuration |
| DeleteCollimatorConfiguration | [DeleteCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationRequest) | [DeleteCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-DeleteCollimatorConfigurationResponse) | RPC method to delete a collimator configuration by ID |
| SearchCollimatorConfiguration | [SearchCollimatorConfigurationRequest](#com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationRequest) | [SearchCollimatorConfigurationResponse](#com-empyreanmed-heracles-collimator_configurations-v1-SearchCollimatorConfigurationResponse) | RPC method to search collimator configurations |

 



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

