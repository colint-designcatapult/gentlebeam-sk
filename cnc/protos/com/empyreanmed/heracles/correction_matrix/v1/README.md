# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/correction_matrix/v1/correction_matrix.proto](#com_empyreanmed_heracles_correction_matrix_v1_correction_matrix-proto)
    - [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix)
  
- [com/empyreanmed/heracles/correction_matrix/v1/correction_matrix_service.proto](#com_empyreanmed_heracles_correction_matrix_v1_correction_matrix_service-proto)
    - [CreateCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixRequest)
    - [CreateCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixResponse)
    - [DeleteCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixRequest)
    - [DeleteCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixResponse)
    - [GetCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixRequest)
    - [GetCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixResponse)
    - [ListCorrectionMatricesRequest](#com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesRequest)
    - [ListCorrectionMatricesResponse](#com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesResponse)
    - [UpdateCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixRequest)
    - [UpdateCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixResponse)
  
    - [CorrectionMatrixService](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrixService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_correction_matrix_v1_correction_matrix-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/correction_matrix/v1/correction_matrix.proto



<a name="com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix"></a>

### CorrectionMatrix
CorrectionMatrix represents the correction matrix for an energy configuration.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for the correction matrix. |
| preset_configuration_id | [int64](#int64) | optional | Foreign key to the preset_configuration_id entity. |
| create_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The creation date of the correction matrix. |
| magnetometer_type | [com.empyreanmed.heracles.enums.v1.MAGNETOMETERTYPE](#com-empyreanmed-heracles-enums-v1-MAGNETOMETERTYPE) | optional | The type of magnetometer. |
| cm11 | [float](#float) | optional | Correction matrix element CM11. |
| cm12 | [float](#float) | optional | Correction matrix element CM12. |
| cm13 | [float](#float) | optional | Correction matrix element CM13. |
| cm21 | [float](#float) | optional | Correction matrix element CM21. |
| cm22 | [float](#float) | optional | Correction matrix element CM22. |
| cm23 | [float](#float) | optional | Correction matrix element CM23. |





 

 

 

 



<a name="com_empyreanmed_heracles_correction_matrix_v1_correction_matrix_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/correction_matrix/v1/correction_matrix_service.proto



<a name="com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixRequest"></a>

### CreateCorrectionMatrixRequest
Request message for creating a new correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrix | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) | optional | The correction matrix to be created. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixResponse"></a>

### CreateCorrectionMatrixResponse
Response message with the created correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrix | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) |  | The created correction matrix. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixRequest"></a>

### DeleteCorrectionMatrixRequest
Request message for deleting a correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the correction matrix to be deleted. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixResponse"></a>

### DeleteCorrectionMatrixResponse
An empty response message for `DeleteCorrectionMatrix`.






<a name="com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixRequest"></a>

### GetCorrectionMatrixRequest
Request message for fetching a single correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the correction matrix to be returned. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixResponse"></a>

### GetCorrectionMatrixResponse
Response message for fetching a single correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrix | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) |  | The correction matrix with the provided ID. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesRequest"></a>

### ListCorrectionMatricesRequest
Request message for listing correction matrices.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| page_size | [int32](#int32) | optional | The maximum number of correction matrices to return. |
| page_token | [string](#string) | optional | A page token received from a previous `ListCorrectionMatrices` call. |
| preset_configuration_id | [int64](#int64) | optional | The preset configuration ID to filter correction matrices by. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesResponse"></a>

### ListCorrectionMatricesResponse
Response message with the listed correction matrices.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrices | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) | repeated | The correction matrices matching the list request. |
| next_page_token | [string](#string) | optional | A token to retrieve the next page of results. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixRequest"></a>

### UpdateCorrectionMatrixRequest
Request message for updating an existing correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrix | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) | optional | The correction matrix to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields to update. |






<a name="com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixResponse"></a>

### UpdateCorrectionMatrixResponse
Response message with the updated correction matrix.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| correction_matrix | [CorrectionMatrix](#com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrix) |  | The updated correction matrix. |





 

 

 


<a name="com-empyreanmed-heracles-correction_matrix-v1-CorrectionMatrixService"></a>

### CorrectionMatrixService
Performs CRUD operations on correction matrices.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListCorrectionMatrices | [ListCorrectionMatricesRequest](#com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesRequest) | [ListCorrectionMatricesResponse](#com-empyreanmed-heracles-correction_matrix-v1-ListCorrectionMatricesResponse) | Lists correction matrices matching request parameters. |
| GetCorrectionMatrix | [GetCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixRequest) | [GetCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-GetCorrectionMatrixResponse) | Returns a single correction matrix. |
| CreateCorrectionMatrix | [CreateCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixRequest) | [CreateCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-CreateCorrectionMatrixResponse) | Creates a new correction matrix. |
| UpdateCorrectionMatrix | [UpdateCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixRequest) | [UpdateCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-UpdateCorrectionMatrixResponse) | Updates a single correction matrix. |
| DeleteCorrectionMatrix | [DeleteCorrectionMatrixRequest](#com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixRequest) | [DeleteCorrectionMatrixResponse](#com-empyreanmed-heracles-correction_matrix-v1-DeleteCorrectionMatrixResponse) | Deletes a single correction matrix. |

 



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

