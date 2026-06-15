# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/logs/v1/log.proto](#com_empyreanmed_heracles_logs_v1_log-proto)
    - [Log](#com-empyreanmed-heracles-logs-v1-Log)
  
- [com/empyreanmed/heracles/logs/v1/log_service.proto](#com_empyreanmed_heracles_logs_v1_log_service-proto)
    - [CreateLogRequest](#com-empyreanmed-heracles-logs-v1-CreateLogRequest)
    - [CreateLogResponse](#com-empyreanmed-heracles-logs-v1-CreateLogResponse)
    - [DeleteLogRequest](#com-empyreanmed-heracles-logs-v1-DeleteLogRequest)
    - [DeleteLogResponse](#com-empyreanmed-heracles-logs-v1-DeleteLogResponse)
    - [GetLogRequest](#com-empyreanmed-heracles-logs-v1-GetLogRequest)
    - [GetLogResponse](#com-empyreanmed-heracles-logs-v1-GetLogResponse)
    - [ListLogsRequest](#com-empyreanmed-heracles-logs-v1-ListLogsRequest)
    - [ListLogsResponse](#com-empyreanmed-heracles-logs-v1-ListLogsResponse)
    - [SearchLogByMessageRequest](#com-empyreanmed-heracles-logs-v1-SearchLogByMessageRequest)
    - [SearchLogByMessageResponse](#com-empyreanmed-heracles-logs-v1-SearchLogByMessageResponse)
    - [UpdateLogRequest](#com-empyreanmed-heracles-logs-v1-UpdateLogRequest)
    - [UpdateLogResponse](#com-empyreanmed-heracles-logs-v1-UpdateLogResponse)
  
    - [LogService](#com-empyreanmed-heracles-logs-v1-LogService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_logs_v1_log-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/logs/v1/log.proto



<a name="com-empyreanmed-heracles-logs-v1-Log"></a>

### Log
Log represents a log in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Primary key. Auto incremental |
| timestamp | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Timestamp |
| severity | [com.empyreanmed.heracles.enums.v1.SEVERITY](#com-empyreanmed-heracles-enums-v1-SEVERITY) | optional | Status Enum |
| type | [com.empyreanmed.heracles.enums.v1.LOGTYPE](#com-empyreanmed-heracles-enums-v1-LOGTYPE) | optional | Type Enum |
| message | [string](#string) | optional | The Log Message |





 

 

 

 



<a name="com_empyreanmed_heracles_logs_v1_log_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/logs/v1/log_service.proto



<a name="com-empyreanmed-heracles-logs-v1-CreateLogRequest"></a>

### CreateLogRequest
Request message for creating a new log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| log | [Log](#com-empyreanmed-heracles-logs-v1-Log) | optional | The log to be created. |






<a name="com-empyreanmed-heracles-logs-v1-CreateLogResponse"></a>

### CreateLogResponse
Response message with the created log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| log | [Log](#com-empyreanmed-heracles-logs-v1-Log) |  | The log that was created. |






<a name="com-empyreanmed-heracles-logs-v1-DeleteLogRequest"></a>

### DeleteLogRequest
Request message for deleting a log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the log to be deleted. |






<a name="com-empyreanmed-heracles-logs-v1-DeleteLogResponse"></a>

### DeleteLogResponse
An empty response message for `DeleteLog`.






<a name="com-empyreanmed-heracles-logs-v1-GetLogRequest"></a>

### GetLogRequest
Request message for fetching a single log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the log to be returned. |






<a name="com-empyreanmed-heracles-logs-v1-GetLogResponse"></a>

### GetLogResponse
Response message for fetching a single log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| log | [Log](#com-empyreanmed-heracles-logs-v1-Log) |  | The log with the ID provided in the request. |






<a name="com-empyreanmed-heracles-logs-v1-ListLogsRequest"></a>

### ListLogsRequest
Request message for listing logs.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| skip | [int32](#int32) | optional | The maximum number of logs to return. The service may return fewer than this value. If unset or zero, all logs will be returned. |
| get | [int32](#int32) | optional | A page token, received from a previous `ListLogs` call. Provide this to retrieve the subsequent page.

When paginating, all other parameters provided to `ListLogs` must match the call that provided the page token. |






<a name="com-empyreanmed-heracles-logs-v1-ListLogsResponse"></a>

### ListLogsResponse
Response message with the listed logs.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| logs | [Log](#com-empyreanmed-heracles-logs-v1-Log) | repeated | The logs matching the list request. The order is unspecified. |
| next_page_token | [string](#string) | optional | A token that can be sent as `page_token` to retrieve the next page of results. If this field is omitted, there are no more results. |






<a name="com-empyreanmed-heracles-logs-v1-SearchLogByMessageRequest"></a>

### SearchLogByMessageRequest
Request message for searching a log by message.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| message | [string](#string) | optional | The message of the log to be searched. |






<a name="com-empyreanmed-heracles-logs-v1-SearchLogByMessageResponse"></a>

### SearchLogByMessageResponse
Response message for searching a log by message.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| logs | [Log](#com-empyreanmed-heracles-logs-v1-Log) | repeated | The logs with the message provided in the request. |






<a name="com-empyreanmed-heracles-logs-v1-UpdateLogRequest"></a>

### UpdateLogRequest
Request message for updating an existing log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| log | [Log](#com-empyreanmed-heracles-logs-v1-Log) | optional | The log to be updated. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | A FieldMask specifying which fields of the log to update. Must not be empty. Fields that have `OUTPUT_ONLY` behavior may not be updated. |






<a name="com-empyreanmed-heracles-logs-v1-UpdateLogResponse"></a>

### UpdateLogResponse
Response message with the updated log.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| log | [Log](#com-empyreanmed-heracles-logs-v1-Log) |  | The updated log. |





 

 

 


<a name="com-empyreanmed-heracles-logs-v1-LogService"></a>

### LogService
Performs CRUD operations on logs.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListLogs | [ListLogsRequest](#com-empyreanmed-heracles-logs-v1-ListLogsRequest) | [ListLogsResponse](#com-empyreanmed-heracles-logs-v1-ListLogsResponse) | Lists logs matching request parameters. |
| GetLog | [GetLogRequest](#com-empyreanmed-heracles-logs-v1-GetLogRequest) | [GetLogResponse](#com-empyreanmed-heracles-logs-v1-GetLogResponse) | Returns a single log. |
| CreateLog | [CreateLogRequest](#com-empyreanmed-heracles-logs-v1-CreateLogRequest) | [CreateLogResponse](#com-empyreanmed-heracles-logs-v1-CreateLogResponse) | Creates a new log. |
| UpdateLog | [UpdateLogRequest](#com-empyreanmed-heracles-logs-v1-UpdateLogRequest) | [UpdateLogResponse](#com-empyreanmed-heracles-logs-v1-UpdateLogResponse) | Updates a single log. |
| DeleteLog | [DeleteLogRequest](#com-empyreanmed-heracles-logs-v1-DeleteLogRequest) | [DeleteLogResponse](#com-empyreanmed-heracles-logs-v1-DeleteLogResponse) | Deletes a single log. |
| SearchLogByMessage | [SearchLogByMessageRequest](#com-empyreanmed-heracles-logs-v1-SearchLogByMessageRequest) | [SearchLogByMessageResponse](#com-empyreanmed-heracles-logs-v1-SearchLogByMessageResponse) | Searches for a log by the message. |

 



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

