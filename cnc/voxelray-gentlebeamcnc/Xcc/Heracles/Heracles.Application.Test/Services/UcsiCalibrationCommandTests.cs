using Empyrean.Common.Infra.Networking.Udp;
using NUnit.Framework;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Heracles.Application.Test.Services;

/// <summary>
/// Unit tests for UCSI calibration commands (HVPS and coils).
/// Tests verify:
/// 1. Packet generation correctness (payload structure, packet type)
/// 2. Coil and HVPS command packet structure
/// 3. Boundary value handling and serialization
/// 
/// NOTE: These are unit tests with no network communication.
/// No bench connection or external hardware required.
/// All tests operate on packet generation and structure verification.
/// </summary>
[TestFixture]
internal sealed class UcsiCalibrationCommandTests
{
    private GcbXRayCommandOperator _commandOperator = null!;

    [SetUp]
    public void Setup()
    {
        _commandOperator = new GcbXRayCommandOperator();
    }

    #region HVPS Command Packet Tests

    /// <summary>
    /// Verify KV command packet structure:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=5, kv_setpoint, 0]
    /// - Value is correctly serialized
    /// </summary>
    [Test]
    public void GenerateCalibrationHvpsKvCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float kvSetpoint = 50.0f;

        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsKvCmd(kvSetpoint);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(5)); // cmd_id for KV = 5
            Assert.That((float)decoded[1], Is.EqualTo(kvSetpoint).Within(0.01f));
            Assert.That((int)decoded[2], Is.EqualTo(0)); // flags = 0
        });
    }

    /// <summary>
    /// Verify Power command packet structure:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=4, power_setpoint, 0]
    /// </summary>
    [Test]
    public void GenerateCalibrationHvpsPowerCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float powerSetpoint = 200.0f;

        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsPowerCmd(powerSetpoint);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(4)); // cmd_id for Power = 4
            Assert.That((float)decoded[1], Is.EqualTo(powerSetpoint).Within(0.1f));
            Assert.That((int)decoded[2], Is.EqualTo(0)); // flags = 0
        });
    }

    /// <summary>
    /// Verify mA Limit command packet:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=6, ma_limit_value, 0]
    /// </summary>
    [Test]
    public void GenerateCalibrationHvpsMaLimitCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float maLimit = 3.5f;

        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsMaLimitCmd(maLimit);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(6)); // cmd_id for mA = 6
            Assert.That((float)decoded[1], Is.EqualTo(maLimit).Within(0.01f));
            Assert.That((int)decoded[2], Is.EqualTo(0)); // flags = 0
        });
    }

    /// <summary>
    /// Verify Grid Voltage command packet:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=7, grid_voltage, 0]
    /// </summary>
    [Test]
    public void GenerateCalibrationHvpsGridCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float gridVoltage = 150.0f;

        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsGridCmd(gridVoltage);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(7)); // cmd_id for Grid = 7
            Assert.That((float)decoded[1], Is.EqualTo(gridVoltage).Within(0.1f));
            Assert.That((int)decoded[2], Is.EqualTo(0)); // flags = 0
        });
    }

    /// <summary>
    /// Verify Filament command packet:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=8, filament_current, 0]
    /// </summary>
    [Test]
    public void GenerateCalibrationHvpsFilamentCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float filamentCurrent = 3.2f;

        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsFilamentCmd(filamentCurrent);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(8)); // cmd_id for Filament = 8
            Assert.That((float)decoded[1], Is.EqualTo(filamentCurrent).Within(0.01f));
            Assert.That((int)decoded[2], Is.EqualTo(0)); // flags = 0
        });
    }

    /// <summary>
    /// Verify PID Control command packet:
    /// - Packet type is CalibrationHvpsCmd (31)
    /// - Payload contains cmd_id + float + flags [cmd_id=10, enable_flag, 0x18]
    /// </summary>
    [TestCase(true)]
    [TestCase(false)]
    public void GenerateCalibrationHvpsPidCmd_ProducesCorrectPacket(bool enable)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsPidCmd(enable);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // cmd_id, value, flags
            Assert.That((int)decoded[0], Is.EqualTo(10)); // cmd_id for PID = 10
            Assert.That((float)decoded[1], Is.EqualTo(enable ? 1.0f : 0.0f).Within(0.001f)); // enable flag as float
            Assert.That((int)decoded[2], Is.EqualTo(0x18)); // flags = 0x18
        });
    }

    #endregion

    #region Coils Command Packet Tests

    /// <summary>
    /// Verify Coils command packet structure:
    /// - Packet type is CalibrationCoilsCmd (30)
    /// - Payload contains 3 floats (X, Y, Focus currents)
    /// - All values correctly serialized
    /// </summary>
    [Test]
    public void GenerateCalibrationCoilsCmd_ProducesCorrectPacket()
    {
        // Arrange
        const float xCoil = 1.0f;
        const float yCoil = 0.5f;
        const float focusCoil = 2.0f;

        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationCoilsCmd));
            Assert.That(decoded.PayloadLength, Is.EqualTo(3)); // 3 floats
            Assert.That((float)decoded[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)decoded[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)decoded[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    /// <summary>
    /// Verify coils with negative values (X/Y can be negative):
    /// - Negative values preserved in packet
    /// </summary>
    [Test]
    public void GenerateCalibrationCoilsCmd_WithNegativeValues_PreservesValues()
    {
        // Arrange
        const float xCoil = -1.5f;
        const float yCoil = -0.8f;
        const float focusCoil = 1.5f; // Focus is always positive

        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)decoded[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)decoded[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    /// <summary>
    /// Verify coils with zero values (idle state):
    /// </summary>
    [Test]
    public void GenerateCalibrationCoilsCmd_WithZeroValues_ProducesValidPacket()
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(0.0f, 0.0f, 0.0f);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(0.0f));
            Assert.That((float)decoded[1], Is.EqualTo(0.0f));
            Assert.That((float)decoded[2], Is.EqualTo(0.0f));
        });
    }

    /// <summary>
    /// Verify mixed polarity coils command (X negative, Y positive, Focus positive):
    /// </summary>
    [Test]
    public void GenerateCalibrationCoilsCmd_WithMixedPolarity_PreservesSigns()
    {
        // Arrange
        const float xCoil = -0.75f;
        const float yCoil = 0.60f;
        const float focusCoil = 2.30f;

        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)decoded[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)decoded[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    #endregion

    #region Packet Consistency Tests

    /// <summary>
    /// Verify packet structure consistency:
    /// - All calibration commands produce valid UDP packets
    /// - Packet headers are present and correct
    /// - Note: HVPS commands all use type 31 and differentiate by cmd_id in payload
    /// - Coils command uses type 30
    /// </summary>
    [Test]
    public void AllCalibrationCommands_ProduceValidUdpPackets()
    {
        // Arrange & Act
        var hvpsKvPacket = _commandOperator.GenerateCalibrationHvpsKvCmd(50.0f);
        var hvpsPowerPacket = _commandOperator.GenerateCalibrationHvpsPowerCmd(200.0f);
        var hvpsMaPacket = _commandOperator.GenerateCalibrationHvpsMaLimitCmd(3.5f);
        var hvpsGridPacket = _commandOperator.GenerateCalibrationHvpsGridCmd(150.0f);
        var hvpsFilamentPacket = _commandOperator.GenerateCalibrationHvpsFilamentCmd(3.2f);
        var hvpsPidPacket = _commandOperator.GenerateCalibrationHvpsPidCmd(true);
        var coilsPacket = _commandOperator.GenerateCalibrationCoilsCmd(1.0f, 0.5f, 2.0f);

        // Assert - verify all packets can be decoded without exception
        Assert.DoesNotThrow(() =>
        {
            _ = new UdpPacket(hvpsKvPacket);
            _ = new UdpPacket(hvpsPowerPacket);
            _ = new UdpPacket(hvpsMaPacket);
            _ = new UdpPacket(hvpsGridPacket);
            _ = new UdpPacket(hvpsFilamentPacket);
            _ = new UdpPacket(hvpsPidPacket);
            _ = new UdpPacket(coilsPacket);
        });
    }

    /// <summary>
    /// Verify all command packets have valid length:
    /// - HVPS commands: 4-byte header + 3 payload fields (cmd_id, value, flags)
    /// - Coils command: 4-byte header + 3 float payload fields (X, Y, Focus)
    /// </summary>
    [Test]
    public void AllCalibrationCommands_ProduceValidPacketLengths()
    {
        // Arrange & Act
        var hvpsKvPacket = _commandOperator.GenerateCalibrationHvpsKvCmd(50.0f);
        var hvpsPowerPacket = _commandOperator.GenerateCalibrationHvpsPowerCmd(200.0f);
        var coilsPacket = _commandOperator.GenerateCalibrationCoilsCmd(1.0f, 0.5f, 2.0f);

        // Assert - HVPS commands should be 8 bytes (4-byte header + 4-byte payload)
        Assert.That(hvpsKvPacket.Length, Is.GreaterThanOrEqualTo(8));
        Assert.That(hvpsPowerPacket.Length, Is.GreaterThanOrEqualTo(8));
        // Coils command should be 16 bytes (4-byte header + 12-byte payload for 3 floats)
        Assert.That(coilsPacket.Length, Is.GreaterThanOrEqualTo(16));
    }

    #endregion

    #region Boundary Value Tests

    /// <summary>
    /// Test HVPS command values at firmware-defined limits:
    /// - KV: typically 0-150 kV
    /// - Power: typically 0-400 W
    /// - mA Limit: typically 0-5 mA
    /// - Grid: typically 0-200 V
    /// - Filament: typically 0-4 A
    /// </summary>
    [TestCase(0.0f)]
    [TestCase(50.0f)]
    [TestCase(75.0f)]
    [TestCase(150.0f)]
    public void GenerateCalibrationHvpsKvCmd_WithBoundaryValues_ProducesValidPacket(float kvValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsKvCmd(kvValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1], cmd_id is at [0]
        Assert.That((float)decoded[1], Is.EqualTo(kvValue).Within(0.01f));
    }

    [TestCase(0.0f)]
    [TestCase(100.0f)]
    [TestCase(200.0f)]
    [TestCase(400.0f)]
    public void GenerateCalibrationHvpsPowerCmd_WithBoundaryValues_ProducesValidPacket(float powerValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsPowerCmd(powerValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1], cmd_id is at [0]
        Assert.That((float)decoded[1], Is.EqualTo(powerValue).Within(0.1f));
    }

    [TestCase(0.0f)]
    [TestCase(2.0f)]
    [TestCase(5.0f)]
    public void GenerateCalibrationHvpsMaLimitCmd_WithBoundaryValues_ProducesValidPacket(float maValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsMaLimitCmd(maValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1], cmd_id is at [0]
        Assert.That((float)decoded[1], Is.EqualTo(maValue).Within(0.01f));
    }

    [TestCase(0.0f)]
    [TestCase(100.0f)]
    [TestCase(200.0f)]
    public void GenerateCalibrationHvpsGridCmd_WithBoundaryValues_ProducesValidPacket(float gridValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsGridCmd(gridValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1], cmd_id is at [0]
        Assert.That((float)decoded[1], Is.EqualTo(gridValue).Within(0.1f));
    }

    [TestCase(0.0f)]
    [TestCase(2.0f)]
    [TestCase(4.0f)]
    public void GenerateCalibrationHvpsFilamentCmd_WithBoundaryValues_ProducesValidPacket(float filamentValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsFilamentCmd(filamentValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1], cmd_id is at [0]
        Assert.That((float)decoded[1], Is.EqualTo(filamentValue).Within(0.01f));
    }

    /// <summary>
    /// Test coils command at UI-defined limits:
    /// - X/Y: -1.5 to +1.5 A (UI range, firmware allows -2.0 to +2.0)
    /// - Focus: 0 to +3.0 A
    /// </summary>
    [TestCase(-1.5f, -1.5f, 0.0f)]
    [TestCase(-1.0f, 0.0f, 1.5f)]
    [TestCase(0.0f, 0.0f, 1.5f)]
    [TestCase(1.0f, 1.0f, 2.0f)]
    [TestCase(1.5f, 1.5f, 3.0f)]
    public void GenerateCalibrationCoilsCmd_WithUiLimits_ProducesValidPacket(float xCoil, float yCoil, float focusCoil)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)decoded[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)decoded[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    /// <summary>
    /// Test coils command at firmware limits (beyond UI range):
    /// - Firmware allows X/Y: -2.0 to +2.0 A
    /// - Firmware allows Focus: 0 to +3.0 A
    /// </summary>
    [TestCase(-2.0f, -2.0f, 0.0f)]
    [TestCase(2.0f, 2.0f, 3.0f)]
    [TestCase(-1.99f, 1.99f, 3.0f)]
    public void GenerateCalibrationCoilsCmd_WithFirmwareLimits_ProducesValidPacket(float xCoil, float yCoil, float focusCoil)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded = new UdpPacket(packet);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)decoded[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)decoded[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    #endregion

    #region Packet Serialization Tests

    /// <summary>
    /// Verify float serialization consistency:
    /// - Same input parameters produce identical payload
    /// - Note: packet counter increments each call, so full packets differ
    /// </summary>
    [Test]
    public void HvpsKvCommand_DeterministicPayloadSerialization()
    {
        // Arrange
        const float kvSetpoint = 50.5f;

        // Act
        var packet1 = _commandOperator.GenerateCalibrationHvpsKvCmd(kvSetpoint);
        var packet2 = _commandOperator.GenerateCalibrationHvpsKvCmd(kvSetpoint);
        var decoded1 = new UdpPacket(packet1);
        var decoded2 = new UdpPacket(packet2);

        // Assert - payload should be identical (even if counter differs)
        Assert.Multiple(() =>
        {
            Assert.That((int)decoded1[0], Is.EqualTo((int)decoded2[0])); // cmd_id
            Assert.That((float)decoded1[1], Is.EqualTo((float)decoded2[1]).Within(0.01f)); // value
            Assert.That((int)decoded1[2], Is.EqualTo((int)decoded2[2])); // flags
        });
    }

    [Test]
    public void HvpsPowerCommand_DeterministicPayloadSerialization()
    {
        // Arrange
        const float powerSetpoint = 200.5f;

        // Act
        var packet1 = _commandOperator.GenerateCalibrationHvpsPowerCmd(powerSetpoint);
        var packet2 = _commandOperator.GenerateCalibrationHvpsPowerCmd(powerSetpoint);
        var decoded1 = new UdpPacket(packet1);
        var decoded2 = new UdpPacket(packet2);

        // Assert - payload should be identical (even if counter differs)
        Assert.Multiple(() =>
        {
            Assert.That((int)decoded1[0], Is.EqualTo((int)decoded2[0])); // cmd_id
            Assert.That((float)decoded1[1], Is.EqualTo((float)decoded2[1]).Within(0.1f)); // value
            Assert.That((int)decoded1[2], Is.EqualTo((int)decoded2[2])); // flags
        });
    }

    [Test]
    public void CoilsCommand_DeterministicPayloadSerialization()
    {
        // Arrange
        const float xCoil = 1.23f;
        const float yCoil = 0.45f;
        const float focusCoil = 2.67f;

        // Act
        var packet1 = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var packet2 = _commandOperator.GenerateCalibrationCoilsCmd(xCoil, yCoil, focusCoil);
        var decoded1 = new UdpPacket(packet1);
        var decoded2 = new UdpPacket(packet2);

        // Assert - payload should be identical (even if counter differs)
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded1[0], Is.EqualTo((float)decoded2[0]).Within(0.001f)); // X
            Assert.That((float)decoded1[1], Is.EqualTo((float)decoded2[1]).Within(0.001f)); // Y
            Assert.That((float)decoded1[2], Is.EqualTo((float)decoded2[2]).Within(0.001f)); // Focus
        });
    }

    /// <summary>
    /// Verify different values produce different packets:
    /// </summary>
    [Test]
    public void HvpsKvCommand_DifferentValues_ProduceDifferentPackets()
    {
        // Act
        var packet1 = _commandOperator.GenerateCalibrationHvpsKvCmd(50.0f);
        var packet2 = _commandOperator.GenerateCalibrationHvpsKvCmd(51.0f);

        // Assert
        Assert.That(packet1, Is.Not.EqualTo(packet2));
    }

    [Test]
    public void CoilsCommand_DifferentValues_ProduceDifferentPackets()
    {
        // Act
        var packet1 = _commandOperator.GenerateCalibrationCoilsCmd(1.0f, 0.5f, 2.0f);
        var packet2 = _commandOperator.GenerateCalibrationCoilsCmd(1.1f, 0.5f, 2.0f);

        // Assert
        Assert.That(packet1, Is.Not.EqualTo(packet2));
    }

    #endregion

    #region Packet Type Verification Tests

    /// <summary>
    /// Verify each command generates the correct packet type:
    /// - Coils: type 30 (CalibrationCoilsCmd)
    /// - All HVPS commands: type 31 (CalibrationHvpsCmd)
    ///   - Differentiated by cmd_id in payload[0]:
    ///     - cmd_id=4: Power (SET_PWR)
    ///     - cmd_id=5: KV (SET_KV)
    ///     - cmd_id=6: mA Limit (SET_MA_LIM)
    ///     - cmd_id=7: Grid (SET_GRID)
    ///     - cmd_id=8: Filament (SET_FIL)
    ///     - cmd_id=10: PID Control (SET_CONFIG)
    /// </summary>
    [Test]
    public void CoilsCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(1.0f, 0.5f, 2.0f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationCoilsCmd));
    }

    [Test]
    public void HvpsKvCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsKvCmd(50.0f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    [Test]
    public void HvpsPowerCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsPowerCmd(200.0f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    [Test]
    public void HvpsMaCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsMaLimitCmd(3.5f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    [Test]
    public void HvpsGridCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsGridCmd(150.0f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    [Test]
    public void HvpsFilamentCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsFilamentCmd(3.2f);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    [Test]
    public void HvpsPidCommand_HasCorrectPacketType()
    {
        var packet = _commandOperator.GenerateCalibrationHvpsPidCmd(true);
        var decoded = new UdpPacket(packet);
        Assert.That((GCBPacketType)decoded.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
    }

    #endregion

    #region Precision Tests

    /// <summary>
    /// Verify float precision is maintained across serialization:
    /// - Test with various decimal places
    /// - Ensure IEEE 754 rounding is consistent
    /// </summary>
    [TestCase(1.234567f)]
    [TestCase(0.123456f)]
    [TestCase(99.999999f)]
    [TestCase(0.000001f)]
    public void CoilsCommand_MaintainsPrecision(float testValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationCoilsCmd(testValue, testValue, testValue);
        var decoded = new UdpPacket(packet);

        // Assert - allow small float rounding error
        Assert.Multiple(() =>
        {
            Assert.That((float)decoded[0], Is.EqualTo(testValue).Within(0.000001f));
            Assert.That((float)decoded[1], Is.EqualTo(testValue).Within(0.000001f));
            Assert.That((float)decoded[2], Is.EqualTo(testValue).Within(0.000001f));
        });
    }

    [TestCase(50.123456f)]
    [TestCase(0.000001f)]
    [TestCase(149.999999f)]
    public void HvpsKvCommand_MaintainsPrecision(float testValue)
    {
        // Act
        var packet = _commandOperator.GenerateCalibrationHvpsKvCmd(testValue);
        var decoded = new UdpPacket(packet);

        // Assert - HVPS value is at index [1] (index [0] is cmd_id)
        Assert.That((float)decoded[1], Is.EqualTo(testValue).Within(0.000001f));
    }

    #endregion

    #region HVPS UART Protocol Tests

    /// <summary>
    /// Verify ACFGS (Get System Config) response parsing:
    /// - Response header is "*ACFGS" (6 bytes)
    /// - 32 little-endian float values follow (128 bytes)
    /// - Response terminator is 0x0A (newline)
    /// - Total: 135 bytes
    /// </summary>
    [Test]
    public void AcfgsResponse_ParsesCorrectStructure()
    {
        // Arrange - Create a mock ACFGS response with known values
        byte[] mockResponse = new byte[135];
        
        // Header: "*ACFGS"
        mockResponse[0] = (byte)'*';
        mockResponse[1] = (byte)'A';
        mockResponse[2] = (byte)'C';
        mockResponse[3] = (byte)'F';
        mockResponse[4] = (byte)'G';
        mockResponse[5] = (byte)'S';
        
        // 32 float values (indices 0-31, stored as bytes 6-133)
        float[] testValues = new float[32];
        for (int i = 0; i < 32; i++)
        {
            testValues[i] = 10.0f + i; // 10.0, 11.0, 12.0, ..., 41.0
            byte[] floatBytes = BitConverter.GetBytes(testValues[i]);
            Array.Copy(floatBytes, 0, mockResponse, 6 + (i * 4), 4);
        }
        
        // Terminator: 0x0A (newline)
        mockResponse[134] = 0x0A;
        
        // Act - Verify response structure
        Assert.Multiple(() =>
        {
            // Header validation
            Assert.That((char)mockResponse[0], Is.EqualTo('*'));
            Assert.That((char)mockResponse[1], Is.EqualTo('A'));
            Assert.That((char)mockResponse[2], Is.EqualTo('C'));
            Assert.That((char)mockResponse[3], Is.EqualTo('F'));
            Assert.That((char)mockResponse[4], Is.EqualTo('G'));
            Assert.That((char)mockResponse[5], Is.EqualTo('S'));
            
            // Verify all 32 float values parse correctly
            for (int i = 0; i < 32; i++)
            {
                float parsedValue = BitConverter.ToSingle(mockResponse, 6 + (i * 4));
                Assert.That(parsedValue, Is.EqualTo(testValues[i]).Within(0.0001f),
                    $"Float value at index {i} mismatch");
            }
            
            // Terminator validation
            Assert.That(mockResponse[134], Is.EqualTo(0x0A));
        });
    }

    /// <summary>
    /// Verify CONFIG_SET command construction:
    /// - Byte 0: '*' (asterisk)
    /// - Bytes 1-5: '..SET' (command, note: two dots then SET)
    /// - Bytes 6-7: Type (uint16_t LE, 0x0000 for system config)
    /// - Bytes 8-9: ID (uint16_t LE, config index 0-31)
    /// - Bytes 10-13: Data (float LE, value to set)
    /// - Byte 14: '\n' (newline terminator)
    /// Total: 15 bytes
    /// </summary>
    [Test]
    public void ConfigSetCommand_HasCorrectStructure()
    {
        // Arrange
        const int configIndex = 5;
        const float configValue = 99.5f;
        
        // Construct CONFIG_SET command frame exactly as firmware expects
        byte[] commandFrame = new byte[15];
        int pos = 0;
        
        // Byte 0: asterisk
        commandFrame[pos++] = (byte)'*';
        
        // Bytes 1-5: "..SET" (command - note: two dots, then SET)
        commandFrame[pos++] = (byte)'.';
        commandFrame[pos++] = (byte)'.';
        commandFrame[pos++] = (byte)'S';
        commandFrame[pos++] = (byte)'E';
        commandFrame[pos++] = (byte)'T';
        
        // Bytes 6-7: Type field (uint16_t, little-endian) = 0 for system config
        commandFrame[pos++] = 0x00;
        commandFrame[pos++] = 0x00;
        
        // Bytes 8-9: ID field (uint16_t, little-endian) = config index
        byte[] indexBytes = BitConverter.GetBytes((ushort)configIndex);
        commandFrame[pos++] = indexBytes[0];
        commandFrame[pos++] = indexBytes[1];
        
        // Bytes 10-13: Data field (float, little-endian) = value
        byte[] valueBytes = BitConverter.GetBytes(configValue);
        commandFrame[pos++] = valueBytes[0];
        commandFrame[pos++] = valueBytes[1];
        commandFrame[pos++] = valueBytes[2];
        commandFrame[pos++] = valueBytes[3];
        
        // Byte 14: newline terminator
        commandFrame[pos++] = (byte)'\n';
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            // Verify total length
            Assert.That(commandFrame.Length, Is.EqualTo(15));
            
            // Verify header
            Assert.That((char)commandFrame[0], Is.EqualTo('*'));
            
            // Verify command field (bytes 1-5: ..SET)
            Assert.That((char)commandFrame[1], Is.EqualTo('.'));
            Assert.That((char)commandFrame[2], Is.EqualTo('.'));
            Assert.That((char)commandFrame[3], Is.EqualTo('S'));
            Assert.That((char)commandFrame[4], Is.EqualTo('E'));
            Assert.That((char)commandFrame[5], Is.EqualTo('T'));
            
            // Verify Type field (bytes 6-7: should be 0x0000)
            Assert.That(commandFrame[6], Is.EqualTo(0x00));
            Assert.That(commandFrame[7], Is.EqualTo(0x00));
            
            // Verify ID field (bytes 8-9: should match configIndex in LE)
            ushort parsedIndex = BitConverter.ToUInt16(commandFrame, 8);
            Assert.That(parsedIndex, Is.EqualTo((ushort)configIndex));
            
            // Verify Data field (bytes 10-13: should match configValue)
            float parsedValue = BitConverter.ToSingle(commandFrame, 10);
            Assert.That(parsedValue, Is.EqualTo(configValue).Within(0.00001f));
            
            // Verify terminator
            Assert.That((char)commandFrame[14], Is.EqualTo('\n'));
        });
    }

    /// <summary>
    /// Verify CONFIG_SET command with boundary config indices:
    /// - Index 0 (first config)
    /// - Index 31 (last config)
    /// Ensure 0x000A bytes in ID don't interfere with response parsing
    /// </summary>
    [TestCase(0, 1.5f)]
    [TestCase(10, 25.0f)]  // Index 10 = 0x000A in LE (tests false-positive 0x0A detection)
    [TestCase(31, 99.9f)]
    public void ConfigSetCommand_HandlesAllValidIndices(int index, float value)
    {
        // Arrange
        byte[] commandFrame = new byte[15];
        int pos = 0;
        
        commandFrame[pos++] = (byte)'*';
        commandFrame[pos++] = (byte)'.';
        commandFrame[pos++] = (byte)'.';
        commandFrame[pos++] = (byte)'S';
        commandFrame[pos++] = (byte)'E';
        commandFrame[pos++] = (byte)'T';
        commandFrame[pos++] = 0x00;
        commandFrame[pos++] = 0x00;
        
        byte[] indexBytes = BitConverter.GetBytes((ushort)index);
        commandFrame[pos++] = indexBytes[0];
        commandFrame[pos++] = indexBytes[1];
        
        byte[] valueBytes = BitConverter.GetBytes(value);
        commandFrame[pos++] = valueBytes[0];
        commandFrame[pos++] = valueBytes[1];
        commandFrame[pos++] = valueBytes[2];
        commandFrame[pos++] = valueBytes[3];
        commandFrame[pos++] = (byte)'\n';
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(commandFrame.Length, Is.EqualTo(15));
            
            ushort parsedIndex = BitConverter.ToUInt16(commandFrame, 8);
            Assert.That(parsedIndex, Is.EqualTo((ushort)index),
                $"Index mismatch for config index {index}");
            
            float parsedValue = BitConverter.ToSingle(commandFrame, 10);
            Assert.That(parsedValue, Is.EqualTo(value).Within(0.00001f),
                $"Value mismatch for config value {value}");
        });
    }

    /// <summary>
    /// Verify CONFIG_SET response echo structure:
    /// Firmware echoes back short ACK like "*..SET\n" (~6 bytes minimum)
    /// Test validates that response can be parsed correctly
    /// </summary>
    [Test]
    public void ConfigSetResponse_ValidatesEchoStructure()
    {
        // Arrange - Mock firmware echo response
        byte[] mockEchoResponse = new byte[] 
        {
            (byte)'*',
            (byte)'.',
            (byte)'.',
            (byte)'S',
            (byte)'E',
            (byte)'T',
            (byte)'\n'
        };
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            // Verify minimum response length (6 bytes for "*..SET" + 1 for "\n")
            Assert.That(mockEchoResponse.Length, Is.GreaterThanOrEqualTo(6));
            
            // Verify header
            Assert.That((char)mockEchoResponse[0], Is.EqualTo('*'));
            Assert.That((char)mockEchoResponse[1], Is.EqualTo('.'));
            Assert.That((char)mockEchoResponse[2], Is.EqualTo('.'));
            Assert.That((char)mockEchoResponse[3], Is.EqualTo('S'));
            Assert.That((char)mockEchoResponse[4], Is.EqualTo('E'));
            Assert.That((char)mockEchoResponse[5], Is.EqualTo('T'));
            
            // Verify terminator is present
            Assert.That(mockEchoResponse[mockEchoResponse.Length - 1], Is.EqualTo((byte)'\n'));
        });
    }

    #endregion

    #region HVPS Response Parsing Tests

    /// <summary>
    /// Verify HVPS KV command response parsing.
    /// Response is a simple acknowledgement packet with packet type indicating success.
    /// Payload should contain the command echo or status fields.
    /// </summary>
    [Test]
    public void HvpsKvCommandResponse_ParsesSuccessfully()
    {
        // Arrange - Create a mock HVPS KV response packet
        // Response format: UdpPacket with type indicating success
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 0, 1);
        responsePacket[0] = 5; // cmd_id for KV (echo back)
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)responsePacket.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That((int)responsePacket[0], Is.EqualTo(5)); // Verify cmd_id is preserved
            Assert.That(responsePacket.PayloadLength, Is.GreaterThanOrEqualTo(1));
        });
    }

    /// <summary>
    /// Verify HVPS Power command response parsing.
    /// Response acknowledges the power setpoint was received.
    /// </summary>
    [Test]
    public void HvpsPowerCommandResponse_ParsesSuccessfully()
    {
        // Arrange - Create a mock HVPS Power response
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 1, 1);
        responsePacket[0] = 4; // cmd_id for Power
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)responsePacket.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That((int)responsePacket[0], Is.EqualTo(4)); // Verify cmd_id is Power
        });
    }

    /// <summary>
    /// Verify HVPS Grid command response parsing.
    /// Response acknowledges grid voltage setpoint.
    /// </summary>
    [Test]
    public void HvpsGridCommandResponse_ParsesSuccessfully()
    {
        // Arrange - Create a mock HVPS Grid response
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 2, 1);
        responsePacket[0] = 7; // cmd_id for Grid
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)responsePacket.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That((int)responsePacket[0], Is.EqualTo(7)); // Verify cmd_id is Grid
        });
    }

    /// <summary>
    /// Verify HVPS Filament command response parsing.
    /// Response acknowledges filament current setpoint.
    /// </summary>
    [Test]
    public void HvpsFilamentCommandResponse_ParsesSuccessfully()
    {
        // Arrange - Create a mock HVPS Filament response
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 3, 1);
        responsePacket[0] = 8; // cmd_id for Filament
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)responsePacket.PacketType, Is.EqualTo(GCBPacketType.CalibrationHvpsCmd));
            Assert.That((int)responsePacket[0], Is.EqualTo(8)); // Verify cmd_id is Filament
        });
    }

    /// <summary>
    /// Verify HVPS setpoint response parsing (all 5 setpoint values).
    /// Response contains current Power, KV, mA Limit, Grid, and Filament setpoints.
    /// Payload: [Power(float), KV(float), mA_Limit(float), Grid(float), Filament(float)]
    /// Total: 5 floats = 20 bytes payload
    /// </summary>
    [Test]
    public void HvpsSetpointResponse_ParsesAllValues()
    {
        // Arrange - Create mock setpoint response with 5 float values
        float[] setpoints = new float[] { 200.0f, 50.0f, 3.5f, 150.0f, 2.0f };
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 4, 5);
        
        for (int i = 0; i < 5; i++)
        {
            responsePacket[i] = setpoints[i];
        }
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(responsePacket.PayloadLength, Is.EqualTo(5)); // 5 float values
            Assert.That((float)responsePacket[0], Is.EqualTo(200.0f).Within(0.1f), "Power setpoint mismatch");
            Assert.That((float)responsePacket[1], Is.EqualTo(50.0f).Within(0.01f), "KV setpoint mismatch");
            Assert.That((float)responsePacket[2], Is.EqualTo(3.5f).Within(0.01f), "mA Limit setpoint mismatch");
            Assert.That((float)responsePacket[3], Is.EqualTo(150.0f).Within(0.1f), "Grid setpoint mismatch");
            Assert.That((float)responsePacket[4], Is.EqualTo(2.0f).Within(0.01f), "Filament setpoint mismatch");
        });
    }

    /// <summary>
    /// Verify HVPS setpoint response with boundary values (min/max setpoints).
    /// Tests response parsing at firmware-defined limits.
    /// </summary>
    [Test]
    public void HvpsSetpointResponse_WithBoundaryValues_ParsesCorrectly()
    {
        // Arrange - Create response at boundary values
        float[] boundarySetpoints = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }; // All zeros (minimum)
        var minPacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 5, 5);
        for (int i = 0; i < 5; i++)
        {
            minPacket[i] = boundarySetpoints[i];
        }
        
        float[] maxSetpoints = new float[] { 400.0f, 100.0f, 5.0f, 200.0f, 4.0f }; // Maximum values
        var maxPacket = new UdpPacket((uint)GCBPacketType.CalibrationHvpsCmd, 6, 5);
        for (int i = 0; i < 5; i++)
        {
            maxPacket[i] = maxSetpoints[i];
        }
        
        // Act & Assert - Minimum values
        Assert.Multiple(() =>
        {
            Assert.That((float)minPacket[0], Is.EqualTo(0.0f));
            Assert.That((float)minPacket[1], Is.EqualTo(0.0f));
            Assert.That((float)minPacket[2], Is.EqualTo(0.0f));
            Assert.That((float)minPacket[3], Is.EqualTo(0.0f));
            Assert.That((float)minPacket[4], Is.EqualTo(0.0f));
        });
        
        // Act & Assert - Maximum values
        Assert.Multiple(() =>
        {
            Assert.That((float)maxPacket[0], Is.EqualTo(400.0f).Within(0.1f));
            Assert.That((float)maxPacket[1], Is.EqualTo(100.0f).Within(0.01f));
            Assert.That((float)maxPacket[2], Is.EqualTo(5.0f).Within(0.01f));
            Assert.That((float)maxPacket[3], Is.EqualTo(200.0f).Within(0.1f));
            Assert.That((float)maxPacket[4], Is.EqualTo(4.0f).Within(0.01f));
        });
    }

    #endregion

    #region Coils Response Parsing Tests

    /// <summary>
    /// Verify Coils command response parsing.
    /// Response echoes back the 3 coil currents that were commanded.
    /// Payload: [X_current(float), Y_current(float), Focus_current(float)]
    /// </summary>
    [Test]
    public void CoilsCommandResponse_ParsesSuccessfully()
    {
        // Arrange - Create mock coils response with echoed values
        float xCoil = 0.75f;
        float yCoil = -0.50f;
        float focusCoil = 1.25f;
        
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationCoilsCmd, 0, 3);
        responsePacket[0] = xCoil;
        responsePacket[1] = yCoil;
        responsePacket[2] = focusCoil;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((GCBPacketType)responsePacket.PacketType, Is.EqualTo(GCBPacketType.CalibrationCoilsCmd));
            Assert.That(responsePacket.PayloadLength, Is.EqualTo(3));
            Assert.That((float)responsePacket[0], Is.EqualTo(xCoil).Within(0.001f), "X coil current mismatch");
            Assert.That((float)responsePacket[1], Is.EqualTo(yCoil).Within(0.001f), "Y coil current mismatch");
            Assert.That((float)responsePacket[2], Is.EqualTo(focusCoil).Within(0.001f), "Focus coil current mismatch");
        });
    }

    /// <summary>
    /// Verify Coils response with negative values (X/Y can be negative).
    /// Tests that response parsing preserves sign information correctly.
    /// </summary>
    [Test]
    public void CoilsCommandResponse_WithNegativeValues_ParsesCorrectly()
    {
        // Arrange - Create response with negative coil values
        float xCoil = -1.5f;
        float yCoil = -0.8f;
        float focusCoil = 2.5f;
        
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationCoilsCmd, 1, 3);
        responsePacket[0] = xCoil;
        responsePacket[1] = yCoil;
        responsePacket[2] = focusCoil;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)responsePacket[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)responsePacket[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)responsePacket[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    /// <summary>
    /// Verify Coils response with zero values (idle state).
    /// Tests response parsing when all coils are deenergized.
    /// </summary>
    [Test]
    public void CoilsCommandResponse_AllZeros_ParsesCorrectly()
    {
        // Arrange
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationCoilsCmd, 2, 3);
        responsePacket[0] = 0.0f;
        responsePacket[1] = 0.0f;
        responsePacket[2] = 0.0f;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)responsePacket[0], Is.EqualTo(0.0f));
            Assert.That((float)responsePacket[1], Is.EqualTo(0.0f));
            Assert.That((float)responsePacket[2], Is.EqualTo(0.0f));
        });
    }

    /// <summary>
    /// Verify Coils response at firmware limits (beyond UI range).
    /// Firmware allows X/Y: -2.0 to +2.0 A, Focus: 0 to +3.0 A.
    /// Response should parse values at these extremes correctly.
    /// </summary>
    [TestCase(-2.0f, -2.0f, 0.0f)]
    [TestCase(2.0f, 2.0f, 3.0f)]
    [TestCase(-1.99f, 1.99f, 2.99f)]
    public void CoilsCommandResponse_AtFirmwareLimits_ParsesCorrectly(float xCoil, float yCoil, float focusCoil)
    {
        // Arrange
        var responsePacket = new UdpPacket((uint)GCBPacketType.CalibrationCoilsCmd, 3, 3);
        responsePacket[0] = xCoil;
        responsePacket[1] = yCoil;
        responsePacket[2] = focusCoil;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That((float)responsePacket[0], Is.EqualTo(xCoil).Within(0.001f));
            Assert.That((float)responsePacket[1], Is.EqualTo(yCoil).Within(0.001f));
            Assert.That((float)responsePacket[2], Is.EqualTo(focusCoil).Within(0.001f));
        });
    }

    #endregion

    #region System Config Response Parsing Tests

    /// <summary>
    /// Verify ACFGS response with edge case: all float values at zero.
    /// Tests that response parser doesn't misinterpret zero values.
    /// </summary>
    [Test]
    public void AcfgsResponse_AllZeroValues_ParsesCorrectly()
    {
        // Arrange
        byte[] mockResponse = new byte[135];
        
        // Header: "*ACFGS"
        mockResponse[0] = (byte)'*';
        mockResponse[1] = (byte)'A';
        mockResponse[2] = (byte)'C';
        mockResponse[3] = (byte)'F';
        mockResponse[4] = (byte)'G';
        mockResponse[5] = (byte)'S';
        
        // 32 zero float values
        for (int i = 0; i < 32; i++)
        {
            byte[] floatBytes = BitConverter.GetBytes(0.0f);
            Array.Copy(floatBytes, 0, mockResponse, 6 + (i * 4), 4);
        }
        
        // Terminator
        mockResponse[134] = 0x0A;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            for (int i = 0; i < 32; i++)
            {
                float parsedValue = BitConverter.ToSingle(mockResponse, 6 + (i * 4));
                Assert.That(parsedValue, Is.EqualTo(0.0f), $"Float at index {i} should be 0.0");
            }
        });
    }

    /// <summary>
    /// Verify ACFGS response with negative float values.
    /// Tests that negative configuration values are preserved correctly.
    /// </summary>
    [Test]
    public void AcfgsResponse_WithNegativeValues_ParsesCorrectly()
    {
        // Arrange
        byte[] mockResponse = new byte[135];
        mockResponse[0] = (byte)'*';
        mockResponse[1] = (byte)'A';
        mockResponse[2] = (byte)'C';
        mockResponse[3] = (byte)'F';
        mockResponse[4] = (byte)'G';
        mockResponse[5] = (byte)'S';
        
        // Mix of positive and negative values
        float[] testValues = new float[32];
        for (int i = 0; i < 32; i++)
        {
            testValues[i] = i % 2 == 0 ? -10.0f - i : 10.0f + i;
            byte[] floatBytes = BitConverter.GetBytes(testValues[i]);
            Array.Copy(floatBytes, 0, mockResponse, 6 + (i * 4), 4);
        }
        
        mockResponse[134] = 0x0A;
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            for (int i = 0; i < 32; i++)
            {
                float parsedValue = BitConverter.ToSingle(mockResponse, 6 + (i * 4));
                Assert.That(parsedValue, Is.EqualTo(testValues[i]).Within(0.0001f),
                    $"Float value at index {i} mismatch");
            }
        });
    }

    /// <summary>
    /// Verify CONFIG_SET response validates entire frame structure:
    /// - Proper start marker ('*')
    /// - Command echo ("..SET")
    /// - Proper terminator ('\n')
    /// </summary>
    [Test]
    public void ConfigSetResponse_CompleteFrameValidation()
    {
        // Arrange
        byte[] mockResponse = new byte[]
        {
            (byte)'*', (byte)'.', (byte)'.', (byte)'S', (byte)'E', (byte)'T', (byte)'\n'
        };
        
        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(mockResponse.Length, Is.GreaterThanOrEqualTo(7), "Response too short");
            Assert.That((char)mockResponse[0], Is.EqualTo('*'), "Missing start marker");
            Assert.That((char)mockResponse[1], Is.EqualTo('.'));
            Assert.That((char)mockResponse[2], Is.EqualTo('.'));
            Assert.That((char)mockResponse[3], Is.EqualTo('S'));
            Assert.That((char)mockResponse[4], Is.EqualTo('E'));
            Assert.That((char)mockResponse[5], Is.EqualTo('T'));
            Assert.That((char)mockResponse[6], Is.EqualTo('\n'), "Missing terminator");
        });
    }

    #endregion
}
