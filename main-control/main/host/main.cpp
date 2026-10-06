#include <chrono>
#include <cstdint>
#include <memory>
#include <stdexcept>
#include <string>
#include <mutex>
#include <thread>

#include <CLI11.hpp>
#include <nlohmann/json.hpp>
#include <cpp-httplib/httplib.h>

#include "hardware_backend.h"
#include "hvps_backend.h"
#include "network_backend.h"
#include "uart_backend.h"

extern "C" {
void gcb_main();
}

namespace {

using json = nlohmann::json;

std::mutex io_model_mutex;
GcbIoModel io_model_snapshot{};
json pending_io_model_patch = json::object();


template <typename Value>
void update_if_present(const json &object, const char *name, Value &target)
{
    const auto field = object.find(name);
    if (field != object.end()) {
        target = field->get<Value>();
    }
}


template <typename Value, size_t Size>
void update_array_if_present(const json &object, const char *name, Value (&target)[Size])
{
    const auto field = object.find(name);
    if (field == object.end()) {
        return;
    }
    if (!field->is_array() || field->size() != Size) {
        throw std::invalid_argument(std::string(name) + " must contain " + std::to_string(Size) + " elements");
    }
    for (size_t index = 0; index < Size; index++) {
        target[index] = (*field)[index].get<Value>();
    }
}


void apply_timer_patch(const json &patch, GcbTimerModel &timer)
{
    update_if_present(patch, "state", timer.state);
    update_if_present(patch, "time_raw", timer.time_raw);
    update_if_present(patch, "simulate", timer.simulate);
    update_if_present(patch, "response_suppressed", timer.response_suppressed);
    update_if_present(patch, "checksum_corrupted", timer.checksum_corrupted);
    update_if_present(patch, "tick_remainder", timer.tick_remainder);
    update_array_if_present(patch, "rx_buf", timer.rx_buf);
}


void apply_coil_adc_patch(const json &patch, GcbCoilAdcModel &coil)
{
    update_if_present(patch, "temperature", coil.temperature);
    update_if_present(patch, "f_voltage", coil.f_voltage);
    update_if_present(patch, "y_voltage", coil.y_voltage);
    update_if_present(patch, "x_voltage", coil.x_voltage);
    update_if_present(patch, "f_current", coil.f_current);
    update_if_present(patch, "y_current", coil.y_current);
    update_if_present(patch, "x_current", coil.x_current);
}


void apply_system_adc_patch(const json &patch, GcbSystemAdcModel &system)
{
    update_if_present(patch, "voltage_12", system.voltage_12);
    update_if_present(patch, "voltage_5", system.voltage_5);
    update_if_present(patch, "voltage_3p3", system.voltage_3p3);
    update_if_present(patch, "ion_pump_current_1", system.ion_pump_current_1);
    update_if_present(patch, "ion_pump_current_2", system.ion_pump_current_2);
    update_if_present(patch, "ion_pump_voltage", system.ion_pump_voltage);
    update_if_present(patch, "cabinet_thermistor", system.cabinet_thermistor);
    update_if_present(patch, "heatsink_thermistor", system.heatsink_thermistor);
}


void apply_ion_repeller_adc_patch(const json &patch, GcbIonRepellerAdcModel &ion_repeller)
{
    update_if_present(patch, "repeller_voltage", ion_repeller.repeller_voltage);
    update_if_present(patch, "repeller_current", ion_repeller.repeller_current);
}


void apply_fan_dac_patch(const json &patch, GcbFanDacModel &fan)
{
    update_if_present(patch, "heatsink", fan.heatsink);
    update_if_present(patch, "cabinet", fan.cabinet);
    update_if_present(patch, "pump", fan.pump);
}


void apply_coil_dac_patch(const json &patch, GcbCoilDacModel &coil)
{
    update_if_present(patch, "x", coil.x);
    update_if_present(patch, "y", coil.y);
    update_if_present(patch, "f", coil.f);
    update_if_present(patch, "simulate", coil.simulate);
}

template <typename Setter>
void update_bool_if_present(const json &object, const char *name, Setter setter)
{
    const auto field = object.find(name);
    if (field != object.end()) {
        setter(field->get<bool>());
    }
}


struct GpioField {
    const char *port_name;
    const char *field_name;
    uint8_t port;
    uint8_t bit;
};

constexpr GpioField gpio_fields[] = {
    {"port_a", "io_ext_wd_rst", 0, 0}, {"port_a", "io_indicators_en", 0, 1},
    {"port_a", "io_timers_start_n", 0, 2}, {"port_a", "pa3", 0, 3},
    {"port_a", "pa4", 0, 4}, {"port_a", "pa5", 0, 5}, {"port_a", "pa6", 0, 6},
    {"port_a", "io_qc_en", 0, 8}, {"port_a", "pa9", 0, 9}, {"port_a", "pa10", 0, 10},
    {"port_a", "io_hvps_rdy_n", 0, 11}, {"port_a", "io_hvps_warning", 0, 12},
    {"port_a", "io_ac_fault", 0, 13}, {"port_a", "io_hv_on", 0, 14},
    {"port_a", "io_grid_off", 0, 15}, {"port_a", "io_hv_en", 0, 16},
    {"port_a", "io_grid_en_n", 0, 17}, {"port_a", "io_emission_en", 0, 18},
    {"port_a", "io_hs_fan_en", 0, 19}, {"port_a", "io_cb_fan_en", 0, 20},
    {"port_a", "io_pump_en", 0, 21}, {"port_a", "io_ion_pump_en", 0, 23},
    {"port_a", "io_ion_repeller_en", 0, 24}, {"port_a", "pa25", 0, 25},
    {"port_a", "pa26", 0, 26}, {"port_a", "pa27", 0, 27}, {"port_a", "pa28", 0, 28},
    {"port_a", "card_detect_0", 0, 29}, {"port_a", "pa30", 0, 30}, {"port_a", "pa31", 0, 31},
    {"port_b", "pb0", 1, 0}, {"port_b", "pb1", 1, 1},
    {"port_b", "io_coil_x_dir_n", 1, 2}, {"port_b", "io_coil_y_dir_n", 1, 3},
    {"port_c", "io_door_closed", 2, 0}, {"port_c", "io_drive_sys_locked", 2, 1},
    {"port_c", "io_base_estop_n", 2, 2}, {"port_c", "io_remote_estop_n", 2, 3},
    {"port_c", "io_kuka_fault_1_n", 2, 4}, {"port_c", "io_kuka_fault_2_n", 2, 5},
    {"port_c", "io_water_level", 2, 6}, {"port_c", "io_ion_pump_hvon", 2, 7},
    {"port_c", "io_timer_fault_1_n", 2, 8}, {"port_c", "io_timer_fault2_n", 2, 9},
    {"port_c", "io_hvps_fault_n", 2, 10}, {"port_c", "io_cooler_fault_n", 2, 11},
    {"port_c", "io_water_temp_fault_n", 2, 12}, {"port_c", "io_wd_fault_n", 2, 13},
    {"port_c", "io_mcu_fault_n", 2, 14}, {"port_c", "spare_interlock_1", 2, 15},
    {"port_c", "io_master_fault_n", 2, 16}, {"port_c", "io_clear_fault", 2, 17},
    {"port_c", "io_remote_key", 2, 18}, {"port_c", "io_base_key", 2, 19},
    {"port_c", "io_coil_dac_ldac_n", 2, 20}, {"port_c", "io_coil_dac_clr_n", 2, 21},
    {"port_c", "io_coil_dac_rdy_n", 2, 22}, {"port_c", "io_coil_dac_cs_n", 2, 23},
    {"port_c", "pc24", 2, 24}, {"port_c", "pc26", 2, 26}, {"port_c", "pc27", 2, 27},
    {"port_c", "io_fan_dac_ldac_n", 2, 28}, {"port_c", "io_fan_dac_clr_n", 2, 29},
    {"port_c", "io_fan_dac_rdy_n", 2, 30}, {"port_c", "io_fan_dac_cs_n", 2, 31},
    {"port_d", "pd0", 3, 0}, {"port_d", "pd1", 3, 1}, {"port_d", "pd2", 3, 2},
    {"port_d", "pd3", 3, 3}, {"port_d", "pd4", 3, 4}, {"port_d", "pd5", 3, 5},
    {"port_d", "pd6", 3, 6}, {"port_d", "pd7", 3, 7}, {"port_d", "pd8", 3, 8},
    {"port_d", "pd9", 3, 9}, {"port_d", "phy_reset_pin", 3, 10},
    {"port_d", "io_led1", 3, 12}, {"port_d", "io_led2", 3, 13},
    {"port_d", "io_led3", 3, 14}, {"port_d", "io_led4", 3, 15},
    {"port_d", "io_led5", 3, 16}, {"port_d", "io_led6", 3, 17},
    {"port_d", "pd18", 3, 18}, {"port_d", "pd19", 3, 19},
    {"port_d", "pd25", 3, 25}, {"port_d", "pd26", 3, 26},
    {"port_d", "pd27", 3, 27}, {"port_d", "pd28", 3, 28},
    {"port_d", "io_remote_led_1", 3, 29}, {"port_d", "io_remote_led_2", 3, 30},
};

void apply_pins_patch(const json &patch, GcbGpioModel &gpio)
{
    if (!patch.is_object()) {
        throw std::invalid_argument("pins must be a JSON object");
    }
    for (const GpioField &field : gpio_fields) {
        const auto port = patch.find(field.port_name);
        if (port == patch.end()) {
            continue;
        }
        if (!port->is_object()) {
            throw std::invalid_argument(std::string(field.port_name) + " must be a JSON object");
        }
        update_bool_if_present(*port, field.field_name, [&gpio, &field](bool value) {
            const uint32_t mask = UINT32_C(1) << field.bit;
            if (value) {
                gpio.port_levels[field.port] |= mask;
            } else {
                gpio.port_levels[field.port] &= ~mask;
            }
        });
    }
}


void apply_model_patch(const json &patch, GcbIoModel &model)
{
    if (!patch.is_object()) {
        throw std::invalid_argument("I/O model patch must be a JSON object");
    }
    if (const auto field = patch.find("backup_timer1"); field != patch.end()) {
        apply_timer_patch(*field, model.backup_timer1);
    }
    if (const auto field = patch.find("backup_timer2"); field != patch.end()) {
        apply_timer_patch(*field, model.backup_timer2);
    }
    if (const auto adcs = patch.find("adcs"); adcs != patch.end()) {
        if (!adcs->is_object()) {
            throw std::invalid_argument("adcs must be a JSON object");
        }
        if (const auto coil = adcs->find("coil"); coil != adcs->end()) {
            apply_coil_adc_patch(*coil, model.adcs.coil);
        }
        if (const auto system = adcs->find("system"); system != adcs->end()) {
            apply_system_adc_patch(*system, model.adcs.system);
        }
        if (const auto ion_repeller = adcs->find("ion_repeller"); ion_repeller != adcs->end()) {
            apply_ion_repeller_adc_patch(*ion_repeller, model.adcs.ion_repeller);
        }
    }
    if (const auto dac = patch.find("dac"); dac != patch.end()) {
        if (!dac->is_object()) {
            throw std::invalid_argument("dac must be a JSON object");
        }
        if (const auto fan = dac->find("fan"); fan != dac->end()) {
            apply_fan_dac_patch(*fan, model.dac.fan);
        }
        if (const auto coil = dac->find("coil"); coil != dac->end()) {
            apply_coil_dac_patch(*coil, model.dac.coil);
        }
    }
    if (const auto gpio = patch.find("gpio"); gpio != patch.end()) {
        if (!gpio->is_object()) {
            throw std::invalid_argument("gpio must be a JSON object");
        }
        update_array_if_present(*gpio, "port_levels", model.gpio.port_levels);
        update_if_present(*gpio, "simulate", model.gpio.simulate);
        if (const auto pins = gpio->find("pins"); pins != gpio->end()) {
            apply_pins_patch(*pins, model.gpio);
        }
    }
}


json timer_json(const GcbTimerModel &timer)
{
    return {
        {"state", timer.state},
        {"time_raw", timer.time_raw},
        {"simulate", timer.simulate},
        {"response_suppressed", timer.response_suppressed},
        {"checksum_corrupted", timer.checksum_corrupted},
        {"tick_remainder", timer.tick_remainder},
        {"rx_buf", timer.rx_buf},
    };
}

json pins_json(const GcbGpioModel &gpio)
{
    json pins = {
        {"port_a", json::object()},
        {"port_b", json::object()},
        {"port_c", json::object()},
        {"port_d", json::object()},
    };
    for (const GpioField &field : gpio_fields) {
        pins[field.port_name][field.field_name] =
            (gpio.port_levels[field.port] & (UINT32_C(1) << field.bit)) != 0;
    }
    return pins;
}


json model_json(const GcbIoModel &model)
{
    return {
        {"backup_timer1", timer_json(model.backup_timer1)},
        {"backup_timer2", timer_json(model.backup_timer2)},
        {"adcs", {
            {"coil", {
                {"temperature", model.adcs.coil.temperature},
                {"f_voltage", model.adcs.coil.f_voltage},
                {"y_voltage", model.adcs.coil.y_voltage},
                {"x_voltage", model.adcs.coil.x_voltage},
                {"f_current", model.adcs.coil.f_current},
                {"y_current", model.adcs.coil.y_current},
                {"x_current", model.adcs.coil.x_current},
            }},
            {"system", {
                {"voltage_12", model.adcs.system.voltage_12},
                {"voltage_5", model.adcs.system.voltage_5},
                {"voltage_3p3", model.adcs.system.voltage_3p3},
                {"ion_pump_current_1", model.adcs.system.ion_pump_current_1},
                {"ion_pump_current_2", model.adcs.system.ion_pump_current_2},
                {"ion_pump_voltage", model.adcs.system.ion_pump_voltage},
                {"cabinet_thermistor", model.adcs.system.cabinet_thermistor},
                {"heatsink_thermistor", model.adcs.system.heatsink_thermistor},
            }},
            {"ion_repeller", {
                {"repeller_voltage", model.adcs.ion_repeller.repeller_voltage},
                {"repeller_current", model.adcs.ion_repeller.repeller_current},
            }},
        }},
        {"dac", {
            {"fan", {
                {"heatsink", model.dac.fan.heatsink},
                {"cabinet", model.dac.fan.cabinet},
                {"pump", model.dac.fan.pump},
            }},
            {"coil", {
                {"x", model.dac.coil.x},
                {"y", model.dac.coil.y},
                {"f", model.dac.coil.f},
                {"simulate", model.dac.coil.simulate},
            }},
        }},
        {"gpio", {
            {"port_levels", model.gpio.port_levels},
            {"simulate", model.gpio.simulate},
            {"pins", pins_json(model.gpio)},
        }},
    };
}

void merge_patch(json &target, const json &patch)
{
    for (auto field = patch.begin(); field != patch.end(); ++field) {
        if (field->is_object() && target.contains(field.key()) && target[field.key()].is_object()) {
            merge_patch(target[field.key()], *field);
        } else {
            target[field.key()] = *field;
        }
    }
}


void start_http_server(uint16_t port)
{
    httplib::Server server;
    server.Get("/io-model", [](const httplib::Request &, httplib::Response &response) {
        std::lock_guard lock(io_model_mutex);
        response.set_content(model_json(io_model_snapshot).dump(), "application/json");
    });
    server.Post("/io-model", [](const httplib::Request &request, httplib::Response &response) {
        try {
            const json patch = json::parse(request.body);
            GcbIoModel validation_model{};
            apply_model_patch(patch, validation_model);
            {
                std::lock_guard lock(io_model_mutex);
                merge_patch(pending_io_model_patch, patch);
            }
            response.status = 202;
            response.set_content(R"({"status":"queued"})", "application/json");
        } catch (const std::exception &error) {
            response.status = 400;
            response.set_content(json({{"error", error.what()}}).dump(), "application/json");
        }
    });
    server.listen("127.0.0.1", port);
}

} // namespace

extern "C" uint32_t sys_now(void)
{
    using clock = std::chrono::steady_clock;
    static const auto start = clock::now();
    return static_cast<uint32_t>(
        std::chrono::duration_cast<std::chrono::milliseconds>(clock::now() - start).count());
}


extern "C" void host_io_model_tick(void)
{
    std::lock_guard lock(io_model_mutex);
    if (!pending_io_model_patch.empty()) {
        apply_model_patch(pending_io_model_patch, gIoModel);
        host_apply_gpio_port_levels(gIoModel.gpio.port_levels);
        pending_io_model_patch = json::object();
    }
    io_model_snapshot = gIoModel;
}


int main(int argc, char **argv)
{
    uint16_t command_port = 20;
    uint16_t console_port = 7;
    uint16_t extra_port = 35;
    uint16_t telemetry_port = 40020;
    uint16_t head_port = 41021;
    uint16_t hvps_port = 41022;
    uint16_t io_port = 8080;
    CLI::App app{"Gryphon control host"};
    app.add_option("--command-port", command_port, "UDP command listener port")
        ->check(CLI::Range(1, 65535))->capture_default_str();
    app.add_option("--console-port", console_port, "UDP console listener port (0 selects an ephemeral port)")
        ->check(CLI::Range(0, 65535))->capture_default_str();
    app.add_option("--extra-port", extra_port, "Auxiliary UDP listener port (0 selects an ephemeral port)")
        ->check(CLI::Range(0, 65535))->capture_default_str();
    app.add_option("--telemetry-port", telemetry_port, "UDP telemetry destination port")
        ->check(CLI::Range(1, 65535))->capture_default_str();
    app.add_option("--head-port", head_port, "Head simulator TCP port")
        ->check(CLI::Range(1, 65535))->capture_default_str();
    app.add_option("--hvps-port", hvps_port, "HVPS simulator TCP port")
        ->check(CLI::Range(1, 65535))->capture_default_str();
    app.add_option("--io-port", io_port, "HTTP I/O model listener port")
        ->check(CLI::Range(1, 65535))->capture_default_str();
    CLI11_PARSE(app, argc, argv);

    host_configure_udp_ports(command_port, console_port, extra_port, telemetry_port);
    gIoModel.hvps_ops = &HVPS_REAL_OPS;
    const std::unique_ptr<IoDescriptor, decltype(&uart_destroy_descriptor)> hvps_uart(
        uart_create_tcp_descriptor(hvps_port), uart_destroy_descriptor);
    uart_set_descriptor_instance(&HVPS_UART, hvps_uart.get());

    gIoModel.hb_ops = &HB_REAL_OPS;
    const std::unique_ptr<IoDescriptor, decltype(&uart_destroy_descriptor)> head_board_uart(
        uart_create_tcp_descriptor(head_port), uart_destroy_descriptor);
    uart_set_descriptor_instance(&HB_UART, head_board_uart.get());

    std::thread(start_http_server, io_port).detach();
    gcb_main();
}
