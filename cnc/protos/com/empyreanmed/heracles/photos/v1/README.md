# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/photos/v1/photo.proto](#com_empyreanmed_heracles_photos_v1_photo-proto)
    - [Photo](#com-empyreanmed-heracles-photos-v1-Photo)
  
- [com/empyreanmed/heracles/photos/v1/photo_service.proto](#com_empyreanmed_heracles_photos_v1_photo_service-proto)
    - [CreatePhotoRequest](#com-empyreanmed-heracles-photos-v1-CreatePhotoRequest)
    - [CreatePhotoResponse](#com-empyreanmed-heracles-photos-v1-CreatePhotoResponse)
    - [DeletePhotoRequest](#com-empyreanmed-heracles-photos-v1-DeletePhotoRequest)
    - [DeletePhotoResponse](#com-empyreanmed-heracles-photos-v1-DeletePhotoResponse)
    - [GetPhotoRequest](#com-empyreanmed-heracles-photos-v1-GetPhotoRequest)
    - [GetPhotoResponse](#com-empyreanmed-heracles-photos-v1-GetPhotoResponse)
    - [ListPhotosRequest](#com-empyreanmed-heracles-photos-v1-ListPhotosRequest)
    - [ListPhotosResponse](#com-empyreanmed-heracles-photos-v1-ListPhotosResponse)
    - [ReceivePhotoRequest](#com-empyreanmed-heracles-photos-v1-ReceivePhotoRequest)
    - [ReceivePhotoResponse](#com-empyreanmed-heracles-photos-v1-ReceivePhotoResponse)
    - [SendPhotoRequest](#com-empyreanmed-heracles-photos-v1-SendPhotoRequest)
    - [SendPhotoResponse](#com-empyreanmed-heracles-photos-v1-SendPhotoResponse)
    - [UpdatePhotoRequest](#com-empyreanmed-heracles-photos-v1-UpdatePhotoRequest)
    - [UpdatePhotoResponse](#com-empyreanmed-heracles-photos-v1-UpdatePhotoResponse)
  
    - [PhotosService](#com-empyreanmed-heracles-photos-v1-PhotosService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_photos_v1_photo-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/photos/v1/photo.proto



<a name="com-empyreanmed-heracles-photos-v1-Photo"></a>

### Photo
Represents a photo associated with a diagnosis and visit.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Photo id, globally unique |
| diagnosis_id | [int64](#int64) | optional | Diagnosis id associated with the photo |
| visit_id | [int64](#int64) | optional | Visit id associated with the photo |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Photo creation date |
| description | [string](#string) | optional | Description of the photo |
| template_type | [com.empyreanmed.heracles.enums.v1.TEMPLATETYPE](#com-empyreanmed-heracles-enums-v1-TEMPLATETYPE) | optional | Template type of the photo |
| photo_type | [com.empyreanmed.heracles.enums.v1.PHOTOTYPE](#com-empyreanmed-heracles-enums-v1-PHOTOTYPE) | optional | Photo type of the photo |





 

 

 

 



<a name="com_empyreanmed_heracles_photos_v1_photo_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/photos/v1/photo_service.proto



<a name="com-empyreanmed-heracles-photos-v1-CreatePhotoRequest"></a>

### CreatePhotoRequest
Request message for creating a new photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) | optional | Details of the photo to create. |






<a name="com-empyreanmed-heracles-photos-v1-CreatePhotoResponse"></a>

### CreatePhotoResponse
Response message with the created photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) |  | The photo that was created. |






<a name="com-empyreanmed-heracles-photos-v1-DeletePhotoRequest"></a>

### DeletePhotoRequest
Request message for deleting a photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo_id | [int64](#int64) | optional | The ID of the photo to delete. |






<a name="com-empyreanmed-heracles-photos-v1-DeletePhotoResponse"></a>

### DeletePhotoResponse
An empty response message for `DeletePhoto`.






<a name="com-empyreanmed-heracles-photos-v1-GetPhotoRequest"></a>

### GetPhotoRequest
Request message for fetching a single photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo_id | [int64](#int64) | optional | The ID of the photo to fetch. |






<a name="com-empyreanmed-heracles-photos-v1-GetPhotoResponse"></a>

### GetPhotoResponse
Response message with the fetched photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) |  | The photo with the provided ID. |






<a name="com-empyreanmed-heracles-photos-v1-ListPhotosRequest"></a>

### ListPhotosRequest
Request message for listing photos.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| diagnosis_id | [int64](#int64) | optional | The ID of the diagnosis for which to list photos. |






<a name="com-empyreanmed-heracles-photos-v1-ListPhotosResponse"></a>

### ListPhotosResponse
Response message with the listed photos.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photos | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) | repeated | The photos matching the list request. The order is unspecified. |






<a name="com-empyreanmed-heracles-photos-v1-ReceivePhotoRequest"></a>

### ReceivePhotoRequest
Request message for streaming photo files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo_id | [int64](#int64) | optional | The ID of the photo to be streamed. |






<a name="com-empyreanmed-heracles-photos-v1-ReceivePhotoResponse"></a>

### ReceivePhotoResponse
Response message for streaming photo files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| chunk_data | [bytes](#bytes) |  | Data of a single photo file chunk. |
| chunk_index | [int32](#int32) |  | Chunk index in the sequence. |
| total_chunks | [int32](#int32) |  | The index of the photo in the sequence. |






<a name="com-empyreanmed-heracles-photos-v1-SendPhotoRequest"></a>

### SendPhotoRequest
Request message for sending photo data.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| chunk_data | [bytes](#bytes) |  | The binary data of the photo chunk. |
| photo_id | [int64](#int64) | optional | Metadata about the photo |
| chunk_index | [int32](#int32) |  | Chunk index |
| total_chunks | [int32](#int32) |  | Total number of chunks in the file. |






<a name="com-empyreanmed-heracles-photos-v1-SendPhotoResponse"></a>

### SendPhotoResponse
Response message for acknowledging the received photo files.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| message | [string](#string) |  | Message indicating whether the upload was successful. |
| total_files_received | [int32](#int32) |  | The total number of files received. |






<a name="com-empyreanmed-heracles-photos-v1-UpdatePhotoRequest"></a>

### UpdatePhotoRequest
Request message for updating an existing photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) | optional | The photo to update. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the photo to update. |






<a name="com-empyreanmed-heracles-photos-v1-UpdatePhotoResponse"></a>

### UpdatePhotoResponse
Response message with the updated photo.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| photo | [Photo](#com-empyreanmed-heracles-photos-v1-Photo) |  | The updated photo. |





 

 

 


<a name="com-empyreanmed-heracles-photos-v1-PhotosService"></a>

### PhotosService
Performs operations on photos.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListPhotos | [ListPhotosRequest](#com-empyreanmed-heracles-photos-v1-ListPhotosRequest) | [ListPhotosResponse](#com-empyreanmed-heracles-photos-v1-ListPhotosResponse) | Lists photos for a given patient. |
| GetPhoto | [GetPhotoRequest](#com-empyreanmed-heracles-photos-v1-GetPhotoRequest) | [GetPhotoResponse](#com-empyreanmed-heracles-photos-v1-GetPhotoResponse) | Returns a single photo. |
| CreatePhoto | [CreatePhotoRequest](#com-empyreanmed-heracles-photos-v1-CreatePhotoRequest) | [CreatePhotoResponse](#com-empyreanmed-heracles-photos-v1-CreatePhotoResponse) | Creates a new photo for a patient. |
| UpdatePhoto | [UpdatePhotoRequest](#com-empyreanmed-heracles-photos-v1-UpdatePhotoRequest) | [UpdatePhotoResponse](#com-empyreanmed-heracles-photos-v1-UpdatePhotoResponse) | Updates an existing photo. |
| DeletePhoto | [DeletePhotoRequest](#com-empyreanmed-heracles-photos-v1-DeletePhotoRequest) | [DeletePhotoResponse](#com-empyreanmed-heracles-photos-v1-DeletePhotoResponse) | Deletes a photo. |
| SendPhoto | [SendPhotoRequest](#com-empyreanmed-heracles-photos-v1-SendPhotoRequest) stream | [SendPhotoResponse](#com-empyreanmed-heracles-photos-v1-SendPhotoResponse) | send photo to server. |
| ReceivePhoto | [ReceivePhotoRequest](#com-empyreanmed-heracles-photos-v1-ReceivePhotoRequest) | [ReceivePhotoResponse](#com-empyreanmed-heracles-photos-v1-ReceivePhotoResponse) stream | Receive photo from server. |

 



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

