# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles_robotic_arm/move_commands/v1/move_command_service.proto](#com_empyreanmed_heracles_robotic_arm_move_commands_v1_move_command_service-proto)
    - [ConvertRotateRelativeToPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionRequest)
    - [ConvertRotateRelativeToPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionResponse)
    - [ConvertTranslateRelativeToPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionRequest)
    - [ConvertTranslateRelativeToPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionResponse)
    - [GetJointsPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionRequest)
    - [GetJointsPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionResponse)
    - [GetKeepAliveRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveRequest)
    - [GetKeepAliveResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveResponse)
    - [GetPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionRequest)
    - [GetPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionResponse)
    - [MoveByMatrixActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionRequest)
    - [MoveByMatrixActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionResponse)
    - [MoveCustomActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionRequest)
    - [MoveCustomActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionResponse)
    - [MoveToPositionActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionRequest)
    - [MoveToPositionActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionResponse)
    - [PingActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionRequest)
    - [PingActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionResponse)
    - [RotateRelativeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionRequest)
    - [RotateRelativeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionResponse)
    - [RotateRelativeRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeRequest)
    - [RotateRelativeResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeResponse)
    - [SetOperatingModeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionRequest)
    - [SetOperatingModeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionResponse)
    - [StopMotionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionRequest)
    - [StopMotionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionResponse)
    - [TranslateRelativeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionRequest)
    - [TranslateRelativeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionResponse)
    - [TranslateRelativeRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeRequest)
    - [TranslateRelativeResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeResponse)
  
    - [MoveCommandsService](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCommandsService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_robotic_arm_move_commands_v1_move_command_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles_robotic_arm/move_commands/v1/move_command_service.proto



<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionRequest"></a>

### ConvertRotateRelativeToPositionRequest
Request message for ConvertRotateRelativeToPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| angle_deg | [float](#float) | optional | Angle, deg |
| coordinate_system | [com.empyreanmed.heracles_robotic_arm.coordinate_systems.v1.CoordinateSystem](#com-empyreanmed-heracles_robotic_arm-coordinate_systems-v1-CoordinateSystem) | optional | Coordinate system |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionResponse"></a>

### ConvertRotateRelativeToPositionResponse
Response message for ConvertRotateRelativeToPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| world_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Cartesian word position |
| world_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Angular word position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionRequest"></a>

### ConvertTranslateRelativeToPositionRequest
Request message for ConvertTranslateRelativeToPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| distance_mm | [float](#float) | optional | Distance, mm |
| coordinate_system | [com.empyreanmed.heracles_robotic_arm.coordinate_systems.v1.CoordinateSystem](#com-empyreanmed-heracles_robotic_arm-coordinate_systems-v1-CoordinateSystem) | optional | Coordinate system |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionResponse"></a>

### ConvertTranslateRelativeToPositionResponse
Response message for ConvertTranslateRelativeToPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| world_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Cartesian word position |
| world_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Angular word position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionRequest"></a>

### GetJointsPositionRequest
Request message for GetJointsPosition

Empty






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionResponse"></a>

### GetJointsPositionResponse
Response message for GetJointsPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| feedback_joints_positions_rad | [double](#double) | repeated | Joints positions |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveRequest"></a>

### GetKeepAliveRequest
Request message for GetKeepAlive

Empty






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveResponse"></a>

### GetKeepAliveResponse
Response message for GetKeepAlive


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| status | [com.empyreanmed.heracles_robotic_arm.statuses.v1.Status](#com-empyreanmed-heracles_robotic_arm-statuses-v1-Status) | optional | Status |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionRequest"></a>

### GetPositionRequest
Request message for GetPosition

Empty






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionResponse"></a>

### GetPositionResponse
Response message for GetPosition


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| feedback_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Cartesian position |
| feedback_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Angular position |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionRequest"></a>

### MoveByMatrixActionRequest
Request message for MoveByMatrixAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| matrix4x4 | [com.empyreanmed.heracles_robotic_arm.matrices.v1.Matrix4x4](#com-empyreanmed-heracles_robotic_arm-matrices-v1-Matrix4x4) | optional | Matrix |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionResponse"></a>

### MoveByMatrixActionResponse
Response message for MoveByMatrixAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_joints_positions_rad | [double](#double) | repeated | Feedback joints positions |
| feedback_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Feedback cartesian position |
| feedback_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Feedback angular position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionRequest"></a>

### MoveCustomActionRequest
Define a message with an array of 6 doubles - J1 - J6


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| values_rad | [double](#double) | repeated | This should contain exactly 6 elements |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionResponse"></a>

### MoveCustomActionResponse
Response message for MoveCustomAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_joints_positions_rad | [double](#double) | repeated | Feedback joints positions |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionRequest"></a>

### MoveToPositionActionRequest
Request message for MoveToPositionAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| world_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Cartesian word position |
| world_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Angular word position |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionResponse"></a>

### MoveToPositionActionResponse
Response message for MoveToPositionAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Feedback cartesian position |
| feedback_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Feedback angular position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionRequest"></a>

### PingActionRequest
Request message for PingAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| pongs_amount | [int32](#int32) | optional | Pongs amount |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionResponse"></a>

### PingActionResponse
Response message for PingAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_pong_id | [int32](#int32) | optional | Feedback pong id |
| result_pongs_total | [int32](#int32) | optional | Result pongs total |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionRequest"></a>

### RotateRelativeActionRequest
Request message for RotateRelativeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| deg | [float](#float) | optional | Angle, deg |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionResponse"></a>

### RotateRelativeActionResponse
Response message for RotateRelativeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Feedback cartesian position |
| feedback_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Feedback angular position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeRequest"></a>

### RotateRelativeRequest
Request message for RotateRelative


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| deg | [float](#float) | optional | Angle, deg |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeResponse"></a>

### RotateRelativeResponse
Response message for RotateRelative


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| accepted | [bool](#bool) | optional | Accepted |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionRequest"></a>

### SetOperatingModeActionRequest
Request message for SetOperatingModeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| operating_mode | [com.empyreanmed.heracles_robotic_arm.operating_modes.v1.OperatingMode](#com-empyreanmed-heracles_robotic_arm-operating_modes-v1-OperatingMode) | optional | Operating mode |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionResponse"></a>

### SetOperatingModeActionResponse
Response message for SetOperatingModeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| result_success | [bool](#bool) | optional | Feedback is empty Result success |
| result_details | [string](#string) | optional | Result details |
| result_operating_mode | [com.empyreanmed.heracles_robotic_arm.operating_modes.v1.OperatingMode](#com-empyreanmed-heracles_robotic_arm-operating_modes-v1-OperatingMode) | optional | Result operating mode |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionRequest"></a>

### StopMotionRequest
Request message for StopMotion






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionResponse"></a>

### StopMotionResponse
Response message for StopMotion


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| accepted | [bool](#bool) | optional | Accepted |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionRequest"></a>

### TranslateRelativeActionRequest
Request message for TranslateRelativeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| mm | [float](#float) | optional | Distance, mm |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionResponse"></a>

### TranslateRelativeActionResponse
Response message for TranslateRelativeAction


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| tag | [com.empyreanmed.heracles_robotic_arm.action_response_tags.v1.ActionResponseTag](#com-empyreanmed-heracles_robotic_arm-action_response_tags-v1-ActionResponseTag) | optional | Tag |
| goal_accepted | [bool](#bool) | optional | Goal accepted |
| feedback_position_mm | [com.empyreanmed.heracles_robotic_arm.positions.v1.CartesianPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-CartesianPosition) | optional | Feedback cartesian position |
| feedback_position_deg | [com.empyreanmed.heracles_robotic_arm.positions.v1.AngularPosition](#com-empyreanmed-heracles_robotic_arm-positions-v1-AngularPosition) | optional | Feedback angular position |
| result_success | [bool](#bool) | optional | Result success |
| result_details | [string](#string) | optional | Result details |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeRequest"></a>

### TranslateRelativeRequest
Request message for TranslateRelative


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| axis | [com.empyreanmed.heracles_robotic_arm.axes.v1.Axis](#com-empyreanmed-heracles_robotic_arm-axes-v1-Axis) | optional | Axis |
| mm | [float](#float) | optional | Distance, mm |






<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeResponse"></a>

### TranslateRelativeResponse
Response message for TranslateRelative


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| accepted | [bool](#bool) | optional | Accepted |





 

 

 


<a name="com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCommandsService"></a>

### MoveCommandsService
Basic movement commands service

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| RotateRelative | [RotateRelativeRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeRequest) | [RotateRelativeResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeResponse) | Rotate relative to current position |
| RotateRelativeAction | [RotateRelativeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionRequest) | [RotateRelativeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-RotateRelativeActionResponse) stream | Rotate relative to current position via action |
| TranslateRelative | [TranslateRelativeRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeRequest) | [TranslateRelativeResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeResponse) | Translate relative to current position |
| TranslateRelativeAction | [TranslateRelativeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionRequest) | [TranslateRelativeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-TranslateRelativeActionResponse) stream | Translate relative to current position via action |
| MoveCustomAction | [MoveCustomActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionRequest) | [MoveCustomActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveCustomActionResponse) stream | Move the robot to stored position defined by J1-J6 angular values/position |
| MoveToPositionAction | [MoveToPositionActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionRequest) | [MoveToPositionActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveToPositionActionResponse) stream | Move the robot to word position defined by pose (cartesian [X,Y,Z] and angular [Rx, Ry, Rz] coordinates) |
| MoveByMatrixAction | [MoveByMatrixActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionRequest) | [MoveByMatrixActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-MoveByMatrixActionResponse) stream | Move by matrix relative to current position |
| StopMotion | [StopMotionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionRequest) | [StopMotionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-StopMotionResponse) | Stop any ongoing asynchronous motion |
| PingAction | [PingActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionRequest) | [PingActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-PingActionResponse) stream | Command to check communications |
| SetOperatingModeAction | [SetOperatingModeActionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionRequest) | [SetOperatingModeActionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-SetOperatingModeActionResponse) stream | Set operating mode |
| GetKeepAlive | [GetKeepAliveRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveRequest) | [GetKeepAliveResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetKeepAliveResponse) stream | Get KeepAlive stream |
| GetPosition | [GetPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionRequest) | [GetPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetPositionResponse) | Get the position of the End Effector in world coordinates [X,Y,Z, Rx, Ry, Rz] |
| GetJointsPosition | [GetJointsPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionRequest) | [GetJointsPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-GetJointsPositionResponse) | Get the angular position of joints J1-J6 |
| ConvertTranslateRelativeToPosition | [ConvertTranslateRelativeToPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionRequest) | [ConvertTranslateRelativeToPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertTranslateRelativeToPositionResponse) | Converts translate relative/absolute to word position defined by pose (cartesian [X,Y,Z] and angular [Rx, Ry, Rz] coordinates), no robot motion |
| ConvertRotateRelativeToPosition | [ConvertRotateRelativeToPositionRequest](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionRequest) | [ConvertRotateRelativeToPositionResponse](#com-empyreanmed-heracles_robotic_arm-move_commands-v1-ConvertRotateRelativeToPositionResponse) | Converts rotate relative/absolute to word position defined by pose (cartesian [X,Y,Z] and angular [Rx, Ry, Rz] coordinates), no robot motion |

 



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

