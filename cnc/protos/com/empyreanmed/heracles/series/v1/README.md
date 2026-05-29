# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/series/v1/series.proto](#com_empyreanmed_heracles_series_v1_series-proto)
    - [Series](#com-empyreanmed-heracles-series-v1-Series)
  
- [com/empyreanmed/heracles/series/v1/series_service.proto](#com_empyreanmed_heracles_series_v1_series_service-proto)
    - [CreateSeriesRequest](#com-empyreanmed-heracles-series-v1-CreateSeriesRequest)
    - [CreateSeriesResponse](#com-empyreanmed-heracles-series-v1-CreateSeriesResponse)
    - [DeleteSeriesRequest](#com-empyreanmed-heracles-series-v1-DeleteSeriesRequest)
    - [DeleteSeriesResponse](#com-empyreanmed-heracles-series-v1-DeleteSeriesResponse)
    - [GetDicomRequest](#com-empyreanmed-heracles-series-v1-GetDicomRequest)
    - [GetDicomResponse](#com-empyreanmed-heracles-series-v1-GetDicomResponse)
    - [GetSeriesRequest](#com-empyreanmed-heracles-series-v1-GetSeriesRequest)
    - [GetSeriesResponse](#com-empyreanmed-heracles-series-v1-GetSeriesResponse)
    - [ListSeriesByPatientIdRequest](#com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdRequest)
    - [ListSeriesByPatientIdResponse](#com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdResponse)
    - [ListSeriesRequest](#com-empyreanmed-heracles-series-v1-ListSeriesRequest)
    - [ListSeriesResponse](#com-empyreanmed-heracles-series-v1-ListSeriesResponse)
    - [ReceiveDicomRequest](#com-empyreanmed-heracles-series-v1-ReceiveDicomRequest)
    - [ReceiveDicomResponse](#com-empyreanmed-heracles-series-v1-ReceiveDicomResponse)
    - [SendDicomRequest](#com-empyreanmed-heracles-series-v1-SendDicomRequest)
    - [SendDicomResponse](#com-empyreanmed-heracles-series-v1-SendDicomResponse)
    - [UpdateSeriesRequest](#com-empyreanmed-heracles-series-v1-UpdateSeriesRequest)
    - [UpdateSeriesResponse](#com-empyreanmed-heracles-series-v1-UpdateSeriesResponse)
  
    - [SeriesService](#com-empyreanmed-heracles-series-v1-SeriesService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_series_v1_series-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/series/v1/series.proto



<a name="com-empyreanmed-heracles-series-v1-Series"></a>

### Series
Series is an imaging result in the DICOM format, which is split up to slices.
Every series is associated with a diagnosis.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Series id, globally unique |
| diagnosis_id | [int64](#int64) | optional | The diagnosis associated with this series |
| visit_id | [int64](#int64) | optional | The visit associated with this series |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Series creation date |
| name | [string](#string) | optional | Series name |
| type | [com.empyreanmed.heracles.enums.v1.IMAGETYPE](#com-empyreanmed-heracles-enums-v1-IMAGETYPE) | optional | Series image type |
| location | [string](#string) | optional | Series location |
| lesion_depth | [float](#float) | optional | Depth of the lesion in millimeters |
| description | [string](#string) | optional | Description of the series |
| num_of_instances | [int32](#int32) | optional | Number of instances in the series |





 

 

 

 



<a name="com_empyreanmed_heracles_series_v1_series_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/series/v1/series_service.proto



<a name="com-empyreanmed-heracles-series-v1-CreateSeriesRequest"></a>

### CreateSeriesRequest
Request message for creating a new series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) | optional | Details of the series to create. |






<a name="com-empyreanmed-heracles-series-v1-CreateSeriesResponse"></a>

### CreateSeriesResponse
Response message with the created series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) |  | The series that was created. |






<a name="com-empyreanmed-heracles-series-v1-DeleteSeriesRequest"></a>

### DeleteSeriesRequest
Request message for deleting a series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series_id | [int64](#int64) | optional | The ID of the series to delete. |






<a name="com-empyreanmed-heracles-series-v1-DeleteSeriesResponse"></a>

### DeleteSeriesResponse
An empty response message for `DeleteSeries`.






<a name="com-empyreanmed-heracles-series-v1-GetDicomRequest"></a>

### GetDicomRequest
Request message for fetching DICOM files data.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the series whose DICOM files data will be returned. |






<a name="com-empyreanmed-heracles-series-v1-GetDicomResponse"></a>

### GetDicomResponse
Response message for fetching DICOM files data.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| dicom_file_data | [bytes](#bytes) |  | Data of a single file in the DICOM format. |
| file_index | [int32](#int32) |  | The index of the DICOM file in the sequence. |






<a name="com-empyreanmed-heracles-series-v1-GetSeriesRequest"></a>

### GetSeriesRequest
Request message for fetching a single series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series_id | [int64](#int64) | optional | The ID of the series to fetch. |






<a name="com-empyreanmed-heracles-series-v1-GetSeriesResponse"></a>

### GetSeriesResponse
Response message with the fetched series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) |  | The series with the provided ID. |






<a name="com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdRequest"></a>

### ListSeriesByPatientIdRequest
Request message for listing series by patient ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| patient_id | [int64](#int64) | optional | Filter by patient ID. |






<a name="com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdResponse"></a>

### ListSeriesByPatientIdResponse
Response message with the listed series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) | repeated | The series matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-series-v1-ListSeriesRequest"></a>

### ListSeriesRequest
Request message for listing series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis_id | [int64](#int64) | optional | Filter by diagnosis. |






<a name="com-empyreanmed-heracles-series-v1-ListSeriesResponse"></a>

### ListSeriesResponse
Response message with the listed series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) | repeated | The series matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-series-v1-ReceiveDicomRequest"></a>

### ReceiveDicomRequest
Request message for streaming DICOM files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series_id | [int64](#int64) | optional | The ID of the series whose DICOM files will be streamed. |






<a name="com-empyreanmed-heracles-series-v1-ReceiveDicomResponse"></a>

### ReceiveDicomResponse
Response message for streaming DICOM files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| dicom_file_data | [bytes](#bytes) |  | Data of a single DICOM file. |
| file_index | [int32](#int32) |  | The index of the DICOM file in the series. |






<a name="com-empyreanmed-heracles-series-v1-SendDicomRequest"></a>

### SendDicomRequest
Request message for sending DICOM files data.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| dicom_file_data | [bytes](#bytes) |  | Data of a single file in the DICOM format. |
| file_index | [int32](#int32) |  | The index of the DICOM file in the sequence. |
| series_id | [int64](#int64) | optional | Metadata about the DICOM series (optional, could include patient/series info). |






<a name="com-empyreanmed-heracles-series-v1-SendDicomResponse"></a>

### SendDicomResponse
Response message for acknowledging the received DICOM files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| message | [string](#string) |  | Message indicating whether the upload was successful. |
| total_files_received | [int32](#int32) |  | The total number of files received. |






<a name="com-empyreanmed-heracles-series-v1-UpdateSeriesRequest"></a>

### UpdateSeriesRequest
Request message for updating an existing series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) | optional | The series to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the series to update. |






<a name="com-empyreanmed-heracles-series-v1-UpdateSeriesResponse"></a>

### UpdateSeriesResponse
Response message with the updated series.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| series | [Series](#com-empyreanmed-heracles-series-v1-Series) | optional | The updated series. |





 

 

 


<a name="com-empyreanmed-heracles-series-v1-SeriesService"></a>

### SeriesService
Performs CRUD operations on series.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListSeries | [ListSeriesRequest](#com-empyreanmed-heracles-series-v1-ListSeriesRequest) | [ListSeriesResponse](#com-empyreanmed-heracles-series-v1-ListSeriesResponse) | Lists series matching request parameters. |
| ListSeriesByPatientId | [ListSeriesByPatientIdRequest](#com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdRequest) | [ListSeriesByPatientIdResponse](#com-empyreanmed-heracles-series-v1-ListSeriesByPatientIdResponse) | Lists series by patient ID matching request parameters. |
| GetDicom | [GetDicomRequest](#com-empyreanmed-heracles-series-v1-GetDicomRequest) | [GetDicomResponse](#com-empyreanmed-heracles-series-v1-GetDicomResponse) stream | Streams DICOM files of a given series. |
| GetSeries | [GetSeriesRequest](#com-empyreanmed-heracles-series-v1-GetSeriesRequest) | [GetSeriesResponse](#com-empyreanmed-heracles-series-v1-GetSeriesResponse) | Returns a single series. |
| CreateSeries | [CreateSeriesRequest](#com-empyreanmed-heracles-series-v1-CreateSeriesRequest) | [CreateSeriesResponse](#com-empyreanmed-heracles-series-v1-CreateSeriesResponse) | Creates a new series. |
| UpdateSeries | [UpdateSeriesRequest](#com-empyreanmed-heracles-series-v1-UpdateSeriesRequest) | [UpdateSeriesResponse](#com-empyreanmed-heracles-series-v1-UpdateSeriesResponse) | Updates an existing series. |
| DeleteSeries | [DeleteSeriesRequest](#com-empyreanmed-heracles-series-v1-DeleteSeriesRequest) | [DeleteSeriesResponse](#com-empyreanmed-heracles-series-v1-DeleteSeriesResponse) | Deletes a series. |
| SendDicom | [SendDicomRequest](#com-empyreanmed-heracles-series-v1-SendDicomRequest) stream | [SendDicomResponse](#com-empyreanmed-heracles-series-v1-SendDicomResponse) | send dicom files to server. |
| ReceiveDicom | [ReceiveDicomRequest](#com-empyreanmed-heracles-series-v1-ReceiveDicomRequest) stream | [ReceiveDicomResponse](#com-empyreanmed-heracles-series-v1-ReceiveDicomResponse) | Receive dicom files from server. |

 



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

