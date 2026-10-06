# Main-control system tests

Pytest integration tests exercise the host main-control executable through UDP commands, telemetry, simulated head/HVPS connections, and the host I/O API. Firmware sources are unchanged.

## Run

From this directory, build in an x64 Visual Studio developer shell, then run:

```powershell
cmake --build ..\..\..\out\cmake\host-debug
uv run pytest -ra
```

To select another executable or run only keypad coverage:

```powershell
uv run pytest --host-executable D:\path\to\gryphon_control_host.exe
uv run pytest tests/test_head_interface.py -k keypad -v
```

Pytest defaults to **six parallel workers** using pytest-xdist's `worksteal` scheduler. Use the standard `-n` flag to choose concurrency; no separate cap is enforced:

```powershell
uv run pytest -n 6 -ra
uv run pytest -n 2 -ra
uv run pytest -n 0 -ra  # Serial debugging
```

Each test owns a fresh host, client, and simulators with OS-selected loopback ports. A shared startup lock protects the brief port-reservation handoff; test bodies run concurrently. Startup waits for telemetry, HTTP readiness, and healthy inputs before clearing startup faults. Windows host execution is verified; POSIX execution is not yet verified.

### Per-test execution evidence

Every test records the protocol's key observed values and the expected values or
limits used by its assertions. An `EVIDENCE` block prints after teardown, including
the full test name, parameter ID, StrictDoc UID, and outcome. Output is visible with
normal capture and with `-n 8`; `-s` is not required.

`PASSED` means setup, assertions, and teardown all passed. Assertion failures,
fixture errors, skips, and expected/unexpected passes are labeled separately;
partial observations remain available without claiming success. Evidence uses
the actual assertion snapshots, not additional device queries. Long polling
loops report endpoints, elapsed times, ranges, or state changes rather than every
sample; distinct cases within a test retain separate records.

JUnit reports retain the same block as the `test_evidence` testcase property:

```powershell
uv run pytest -n 8 -ra --junitxml=results.xml
```

When adding tests, request the `evidence: Evidence` fixture and record meaningful
comparisons using `Evidence` from `main_control_system_tests.evidence`. For example:

```python
evidence.record(
    "Coil currents (mA)",
    observed_x_ma=snapshot.x_coil_current, expected_x_ma=x_ma,
    observed_y_ma=snapshot.y_coil_current, expected_y_ma=y_ma,
    observed_focus_ma=snapshot.focus_coil_current, expected_focus_ma=focus_ma,
    tolerance_ma=1,
)
```

Values are snapshotted immediately, so later variable mutation or fixture cleanup
does not alter the evidence. Assertions remain responsible for pass/fail.

### Host executable ports

The host uses vendored CLI11; `gryphon_control_host.exe --help` lists all options. Without options, existing ports are unchanged:

| Option | Default | Endpoint |
| --- | ---: | --- |
| `--command-port` | 20 | UDP command listener |
| `--telemetry-port` | 40020 | UDP telemetry destination |
| `--head-port` | 41021 | Head simulator TCP destination |
| `--hvps-port` | 41022 | HVPS simulator TCP destination |
| `--io-port` | 8080 | HTTP I/O model listener |
| `--console-port` | 7 | UDP console listener |
| `--extra-port` | 35 | Auxiliary UDP listener |

All ports accept 1–65535; console and extra additionally accept `0` for OS-selected ports. Addresses remain loopback-only. For example, with simulators and a telemetry receiver on the matching ports:

```powershell
gryphon_control_host.exe --command-port 42020 --telemetry-port 42021 --head-port 42022 --hvps-port 42023 --io-port 42024 --console-port 0 --extra-port 0
```

The pytest fixtures pass their isolated endpoints automatically, including ephemeral console and extra listeners. Command replies still go to the requesting client's source port, independently of the telemetry destination.

## Fixtures and client

- `client` (`MainControlClient`) sends authenticated commands and retains unmatched responses, telemetry, and fault messages. `Packet` provides the typed packet model; `pc_protocol` decodes normal telemetry, faults, and Beam QA results. Calibration-mode telemetry is unsupported.
- `startup_client` exposes the same connected, settled host before any initialization or fault-clear command. `client` initializes it into healthy Cold.
- `head_interface` (`HeadInterfaceSimulator`) controls coolant, magnetometer, keypad, and QC inputs and records LED/QC commands.
- `hvps` (`HvpsSimulator`) follows requested supply settings and enables, or accepts feedback and fault overrides. It is an ideal supply simulator, not an analog ramp or clinical calibration model.
- `io_model` (`HostIOModel`) reads and patches simulated hardware inputs. `HostFirmware` manages the executable lifecycle.

Use `client.query_telemetry()` or `client.wait_for_telemetry(predicate, timeout=...)` for fresh snapshots and PC keepalive. `telemetry()` and `normal_telemetry()` consume queued unsolicited telemetry, which may be older. `fault_message()` returns diagnostic text and associated metadata. Background transport errors surface through `assert_healthy()` and fixture teardown.

## Inputs and observations

```python
head_interface.set_feedback(flow=4, pressure=4, temperature=20, mag_x_1=5)
head_interface.set_button_pressed(0, True)
head_interface.set_button_pressed(0, False)
head_interface.pause()  # Interrupt head feedback.
head_interface.resume()
head_interface.set_corruption(byte_offset=25)  # Corrupt feedback without fixing CRC.
head_interface.set_corruption()  # Restore valid feedback.
head_interface.wait_for_command(led_sequence=9, after=timestamp)
io_model.set_interlock("io_base_estop_n", False)
io_model.set_interlock("io_base_estop_n", True)
```

HVPS controls include `set_feedback(kv=..., ma=..., heater=..., grid=...)` (`None` restores following), `set_transmitting(False)`, and `set_faults(flag_bits=..., io_bits=...)`. QC controls include `configure_qc(start_delay=..., stop_delay=...)` and `auto_respond=False` for explicit feedback control. These controls change simulated inputs, not main-control state or latched faults directly.

### Host I/O HTTP API

`GET http://127.0.0.1:8080/io-model` returns the model. `POST` accepts a partial JSON patch and returns `202 {"status":"queued"}`; omitted fields are unchanged. For example:

```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:8080/io-model -ContentType application/json -Body '{"adcs":{"system":{"voltage_12":12.75}}}'
```

GPIO values are raw levels (`true` high, `false` low). A single input can be patched with `{"gpio":{"pins":{"port_c":{"io_base_estop_n":false}}}}`.

`io_model.set_gpio_simulation(False)` permits independent input drive, including the buffered master-fault diagnostic, without synthesized latch/timer levels; firmware monitoring remains active. `set_focus_current_ma(value)` holds physical ADC feedback in mA; `restore_coil_feedback()` resumes DAC following. `set_timer_fault(1, checksum_corrupted=True)` corrupts timer integrity; `set_timer_fault(2, response_suppressed=True)` withholds actual transfer completion. `set_timer_fault(timer)` restores healthy communication. These timer controls are also exposed as `checksum_corrupted` and `response_suppressed` under each HTTP `backup_timer1`/`backup_timer2` object.

## Beam QA example

With the `client` and `head_interface` fixtures:

```python
from main_control_system_tests.pc_protocol import OperationalPoint, QcSessionStatus

head_interface.set_feedback(qc_channel_0=200)
head_interface.configure_qc(sample_rate=1000)
session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=2))
client.start_emission(session, beam_qa=True)
result = client.beam_qa_result(timeout=10)
assert result.status == QcSessionStatus.COMPLETE
assert not client.query_telemetry().faults
client.clear_plan()
```

`prepare_emission` clears the previous plan and prepares the requested operating point. `start_emission` arms QC when requested and starts emission. `beam_qa_result` waits for beam-off and retrieves the result. These settings are simulator inputs, not a treatment prescription.

## Observable test protocols

All deadlines below are elapsed seconds from applying the relevant input.

- **Telemetry / TC-H1FWMC-120 → RQ-H1FWMC-83:** verify the 49-field periodic/requested layout, distinct head/HVPS/analog inputs, unsigned QC accumulations, operating states, and nominal 100 Hz cadence. Host cadence checks allow scheduling jitter; they do not certify target real-time timing. Indicator-state, internal-timer-state, and Peltier fields lack meaningful firmware updates and remain a reporting coverage gap.
- **Keypad / TC-H1FWMC-90 → RQ-H1FWMC-2:** press and release laser, LED, camera, function 1, function 2, and zero-G twice; check each resulting telemetry update within 3 seconds. Physical switches and GUI behavior are outside scope.
- **Communications / TC-H1FWMC-91:** interrupted head feedback produces a communication fault within 2 seconds. Corrupted feedback is ignored and produces no communication fault for 3 seconds; valid updates resume after corruption is removed.
- **Fault reporting / TC-H1FWMC-92 → RQ-H1FWMC-6:** induce four distinct coolant/supply faults; check unsolicited and indexed diagnostics, format checksum, raw arguments, captured state/runtime, epoch/index/count, duplicate suppression, four-record capacity, and clearing.
- **Emission indicators / TC-H1FWMC-93 → RQ-H1FWMC-7:** verify external indicator enable and LED6 during an accepted emission and after stop. Door/key-blocked release must never enable emission or indicators. The host observes electrical outputs, not physical light or sound.
- **Interlocks / TC-H1FWMC-94 → RQ-H1FWMC-8:** individually drive all 19 physical Port C inputs; verify reporting within 1 second and required-versus-non-required fault behavior. Door and keys become required in READY; either open emergency stop prevents conditioning/warmup heater and pump energization.
- **Focus coil / TC-H1FWMC-95 → RQ-H1FWMC-9:** use a 500 mA target, sustain deviations of ±149 mA without faults, and require a typed fault for ±151 mA or disconnected feedback. Check measured current, target, 150 mA tolerance, and recovery.
- **Backup timers / TC-H1FWMC-96/97 → RQ-H1FWMC-10/11:** independently corrupt or suppress each timer's responses. Corruption faults within 1 second; silence faults nominally after 0.4–0.8 seconds, observed within 1.5 seconds. Check diagnostics and recovery. The timeout diagnostic names the configured 0.4-second interval, not the entire detection latency.
- **PC queries / TC-H1FWMC-111/112/122 → RQ-H1FWMC-23/81/84/306:** verify indexless scalar reporting, rejection of legacy indexed queries, replacement/confirmation/wipe behavior, correlated command replies, and periodic/requested firmware releases and modes.
- **Fault safety / TC-H1FWMC-123/124/125/126 → RQ-H1FWMC-80/79/78/77:** induce physical emergency-stop and timer communication faults, verify safe outputs and blocked progression, retain fault flags/records for 60 seconds after input recovery, verify accepted/rejected PC clearing and HVPS clear commands, and independently exercise discharge feedback boundaries. State transitions use only UDP commands and GcbIoModel inputs; no state-machine mutation endpoint exists. Queue corruption and invalid-state dispatch are not directly injectable through these interfaces.
- **System monitoring / TC-H1FWMC-132/135/136 → RQ-H1FWMC-71/68/66:** test unwanted cathode current in Cold, Primed, and Ready, HVPS activity LED and communication loss, and cabinet-temperature faults using adjacent ADC values below/above 40 °C. Exact 40 °C is not representable by the simulated 12-bit ADC.
- **Coils / TC-H1FWMC-146/147 → RQ-H1FWMC-55/54:** verify signed deflection and focus setpoint outputs, both deflection axes at ±49 mA healthy deviation and ±51 mA faulty deviation, and disconnected feedback.
- **Backup timers / TC-H1FWMC-148/149 → RQ-H1FWMC-53/52:** verify initialization at delivery duration plus 0.5 seconds, hold in Ready, and physical countdown during emission. Telemetry reports increasing elapsed time; the countdown test subtracts it from the initial backup duration and compares reconstructed remaining time with the physical countdown within 0.1 seconds. Evidence includes raw elapsed readings, converted remaining values, and physical measurements.
- **Coolant / TC-H1FWMC-139:** with the pump on, apply flow **9 LPM**, pressure **9 PSI**, and temperature **40 °C** simultaneously. Require **three distinct faults**: high temperature within **1 second**, high flow and high pressure within **12 seconds**. Separately apply flow **1 LPM**, pressure **1 PSI**, and temperature **20 °C** simultaneously; require **two distinct faults**, low flow and low pressure, within **12 seconds**.
- **Magnetometers / TC-H1FWMC-308:** exercise all three axes on both sensors. The allowed deviation is the greater of 10% of the absolute baseline or 1 µT. Hold a within-limit input for **0.5 seconds** without a fault, then apply a beyond-limit input and require a fault within **1 second**. Check the diagnostic and recovery.
- **LED and Beam QA:** check cold/fault LED commands and QA at 50/70/100 kV with varied simulated detector inputs; verify telemetry, completed results, matching head acquisition totals, absence of faults, emission duration, and plan clearing.
- **Device commands / TC-H1FWMC-156/307:** reject unknown commands and malformed framing, then verify valid identity requests still work; compare startup, periodic, and refreshed main/HVPS releases and modes.
- **Scalar sessions / TC-H1FWMC-113–119/121/129/157–160/162/170/171:** exercise all eight parameter limits, malformed/non-finite values, authentication, argument-free session creation, state restrictions, exact confirmation, interruption invalidation, and ordered HVPS Check/Setup/Ready progression.
- **Emission / TC-H1FWMC-89/100–108 and TCH1FWMC-128:** explicit point release, separate sessions, voltage/filament launch gates, Stop and retained-time resume, voltage/current monitoring, Ready idle timeout, and discharge to Cold. QC coverage includes delayed Active, timeout/Error, stale Active after Clear Faults, zero Idle results, and Stopped/Complete/retained-Error acknowledgments.
- **Startup and heating / TC-H1FWMC-110/127/134/161/163–169:** initialization, configured 1000–3250 mA request limits, invalid targets, state and e-stop restrictions, the real 300-second Primed timeout, and the full 900-second stable conditioning hold. Telemetry queries maintain communication without accelerating firmware time.
- **Filament target timeouts / TC-H1FWMC-98/99 → RQ-H1FWMC-12/13:** independently request conditioning and warmup from Cold, matching the CNC action flow. Hold HVPS feedback at 1500 mA against a 2500 mA target, verify the procedure stays active without premature faults, then require a filament fault after 15 seconds (0.5-second observation tolerance) and transition to Warmup Fault. Evidence reports target/feedback currents, elapsed time, states, and faults. The old 20-/30-minute manual protocols are replaced.
- **Treatment hardware / TC-H1FWMC-109/130/131/133/138/140/141:** HVPS interlock cycling, both backup timers and delivery timer starting with emission, signed coil setpoints, temperature-dependent heatsink fan speeds and emission cooling, and reported coolant flow/temperature faults.
- **Staged timeout / TC-H1FWMC-88 → RQ-H1FWMC-87:** stage a scalar point without releasing it, maintain read-only telemetry keepalive, and require discharge to Cold after 300 seconds without emission. The host currently leaves Staged after approximately 120 seconds; the SRS assertion remains an ordinary failure.
- **Cooling control / TC-H1FWMC-137 → RQ-H1FWMC-65:** verify cabinet fan drive at 3.7/4.1/4.5/4.9 V and both sides of the 2 °C downward hysteresis boundaries. Hold 9 PSI for 12 seconds with the pump off, restore 4 PSI, and verify conditioning starts without stale pressure faults. After Stop, observe the full 180-second pump/1.85 V fan cooldown and subsequent pump-off/0 V output.

The conversions retain ordinary failing assertions for unresolved SRS discrepancies rather than changing firmware or marking them expected failures: undesired-emission monitoring uses a 0.3 mA default and only Setup/Ready instead of the specified 0.2 mA in every non-Emission state; **TC-H1FWMC-100** requires the specified 90-second setup timeout (5-second observation tolerance), while firmware configures 150 seconds. TC-H1FWMC-149 compensates for elapsed backup-timer telemetry when verifying countdown reporting; the firmware representation and RQ-H1FWMC-52 wording are unchanged.

The scalar CNC flow (`Heracles.Indoor` and the `Heracles.External` project in `Heracles.Outdoor`) waits for Cold after emission and explicitly reconfirms a retained point before resume. RQ-H1FWMC-85 now reflects that flow; RQ-H1FWMC-16 includes TC-H1FWMC-103's QC acknowledgment and clearing safeguards. Conditioning completes in firmware Primed; the CNC `Conditioning` action instead awaits StandBy, a separate integration discrepancy not changed here.

TC-H1FWMC-127 tests physical e-stops before requesting warmup and during a held Warmup. The manual protocol's paused-dispatch, accepted-command/pre-entry race is not controllable through the approved interfaces and is not claimed as covered. Coolant sensor loss is represented by UART feedback at main control's boundary, not a physical head-board sensor test.

StrictDoc test definitions and requirement links live in test docstrings. `dcdoc.toml` includes only `tests/**/test_*.py` as test cases in **DHF-GBSK-0074**.
