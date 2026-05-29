
## Installation:

1. Make sure docker server is installed in your computer:
see https://www.docker.com/products/docker-desktop/
2. Make sure that the clones of protos/databases/moses repositories are placed aside from xcc repository, forming the following folder structure:
    - databases
    - moses
    - protos
    - xcc
3. Pull the latest Xcc code
4. Run ```<xcc_root>/Scripts/moses_checkout.cmd``` script to get compatible moses/DB/protos versions
5. Rebuild the Xcc solution
6. Delete the existing moses/heracles-dbservice docker container
7. a. Update the existing database according to the schema.prisma changes:
	- go to <databases_root>
	- run push-herackles-win.cmd
	
   b. If there is any error occurs, you can try to build the Heracles DB docker container from scratch: 
    - go to <databases_root>/deployments/heracles
    - remove "vol" folder there
	- go to <databases_root>
	- run heracles_db.cmd
8. Add a user to the DB to be able to log
    - Connect to the DB via PgAdmin tool for example (port 5433, username=root, password=your_password, as specified in its docker-compose.yml file)
    - Open the user_data table and add a row for a new user there
~~~~sql
INSERT INTO public.user_data (
id, picture, first_name, last_name, username, password, role, email_address) VALUES (
'1'::bigint, 'pic'::character varying, 'Name'::character varying, 'LastName'::character varying, 'Admin'::character varying, 'password'::character varying, 'Administrator'::character varying, 'admin@example.com'::character varying)
 returning id;
~~~~
          
9. Add a Settings record to the DB to be able to get and update it:
~~~~sql
INSERT INTO public.settings (
id, create_date, device_serial, record_and_verify_ip, record_and_verify_port, database_ip, database_port, imaging_headcam_ip, imaging_headcam_port, treatment_headcam_ip, treatment_headcam_port, robotcam_ip, robotcam_port, gcb_telemetry_ip, gcb_telemetry_port, gcb_commands_ip, gcb_commands_port, robotic_ros_ip, robotic_ros_port, data_acquisition_ip, data_acquisition_port, dc_data_reconstruction_ip, dc_data_reconstruction_port, dc_data_progress_websocket_ip, dc_data_progress_websocket_port, dc_data_reconstruction_z_mq_ip, dc_data_reconstruction_z_mq_port, dc_database_ip, dc_database_port, synced) VALUES (
'1'::bigint, '2024-10-30'::date, '1234'::text, '127.0.0.1'::text, '3232'::text, '127.0.0.1'::text, '3232'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, '127.0.0.1'::text, '1234'::text, false::boolean)
 returning id; 
~~~~
 
10. Build the moses docker:
    - go to <moses_root>
    - to enable login service, check jwt flag in docker-compose-heracles.yml, setting it to jwt=1 if needed
	- run heracles-DB-serivce.cmd

## Configuration

1. To be able to access Moses and other remote parts of the system, create/edit appsettings.json "EndPoints" section to have 
proper endpoints. At least, DataCommandsEndPoint and ACB/QCB must be specified. Other endpoints may be configured via Indoor Settings/EndPoint Management.
An example of the configuration:
~~~~json
"EndPoints" : {
    "GCBTelemetryEndPoint": "172.31.1.100:20",
    "GCBCommandsEndPoint": "172.31.1.100:7",
    "AcbCommandsEndPoint": "172.31.1.229:7000",
    "QcbCommandsEndPoint": "172.31.1.231:8000",
    "DataCommandsEndPoint": "172.31.1.222:3232",
    "PhotoAcousticEndPoint": "172.31.1.230:33405",
    "RobotGrpcServerEndPoint": "172.31.1.227:50051"
}
~~~~