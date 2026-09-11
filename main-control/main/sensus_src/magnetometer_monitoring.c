#include <math.h>
#include <stdbool.h>
#include <stdint.h>

#include "faults.h"
#include "magnetometer_monitoring.h"
#include "state_machine.h"
#include "system_parameters.h"

#define MAGNETOMETER_COUNT 2u
#define MAGNETOMETER_AXIS_COUNT 3u

static const uint32_t magnetometer_status_fields[MAGNETOMETER_COUNT][MAGNETOMETER_AXIS_COUNT] = {
	{SS_MAG_X, SS_MAG_Y, SS_MAG_Z},
	{SS_MAG_X2, SS_MAG_Y2, SS_MAG_Z2}
};

static float baseline_samples[MAGNETOMETER_BASELINE_SAMPLE_COUNT][MAGNETOMETER_COUNT][MAGNETOMETER_AXIS_COUNT];
static float magnetometer_baseline[MAGNETOMETER_COUNT][MAGNETOMETER_AXIS_COUNT];
static uint32_t baseline_sample_count = 0u;
static uint32_t baseline_sample_index = 0u;
static uint32_t consecutive_deviation_count[MAGNETOMETER_COUNT][MAGNETOMETER_AXIS_COUNT];
static bool baseline_collection_active = false;
static bool magnetometer_baseline_valid = false;

static float allowed_deviation(float baseline, float percent, float minimum_deviation)
{
	float relative_deviation = fabsf(baseline) * (percent / 100.0f);
	return relative_deviation > minimum_deviation ? relative_deviation : minimum_deviation;
}

static void collect_baseline_sample(void)
{
	if(!baseline_collection_active)
	{
		return;
	}

	for(uint32_t magnetometer = 0; magnetometer < MAGNETOMETER_COUNT; magnetometer++)
	{
		for(uint32_t axis = 0; axis < MAGNETOMETER_AXIS_COUNT; axis++)
		{
			float reading = system_status[magnetometer_status_fields[magnetometer][axis]].f;
			if(!isfinite(reading))
			{
				// Require a complete run of valid post-coil samples.
				baseline_sample_count = 0u;
				baseline_sample_index = 0u;
				return;
			}
			baseline_samples[baseline_sample_index][magnetometer][axis] = reading;
		}
	}

	baseline_sample_index = (baseline_sample_index + 1u) % MAGNETOMETER_BASELINE_SAMPLE_COUNT;
	if(baseline_sample_count < MAGNETOMETER_BASELINE_SAMPLE_COUNT)
	{
		baseline_sample_count++;
	}
}

static void report_deviation_fault(uint32_t magnetometer, uint32_t axis,
	float reading, float baseline, float threshold)
{
	magnetometer_baseline_valid = false;
	report_typed_fault5(
		FAULT_MAGNETOMETER,
		"Magnetometer %u axis %u reading %f uT deviated from baseline %f uT beyond the %f uT limit.",
		MAKE_ARG(magnetometer + 1u),
		MAKE_ARG(axis + 1u),
		MAKE_ARG(reading),
		MAKE_ARG(baseline),
		MAKE_ARG(threshold));
}

void start_magnetometer_baseline_collection(void)
{
	baseline_collection_active = true;
	magnetometer_baseline_valid = false;
	baseline_sample_count = 0u;
	baseline_sample_index = 0u;
}

bool capture_magnetometer_baseline(void)
{
	baseline_collection_active = false;
	magnetometer_baseline_valid = false;

	if(baseline_sample_count < MAGNETOMETER_BASELINE_SAMPLE_COUNT)
	{
		report_typed_fault2(
			FAULT_MAGNETOMETER,
			"Cannot start emission: received %u of %u required post-coil magnetometer samples.",
			MAKE_ARG(baseline_sample_count),
			MAKE_ARG(MAGNETOMETER_BASELINE_SAMPLE_COUNT));
		return false;
	}

	for(uint32_t magnetometer = 0; magnetometer < MAGNETOMETER_COUNT; magnetometer++)
	{
		for(uint32_t axis = 0; axis < MAGNETOMETER_AXIS_COUNT; axis++)
		{
			float sum = 0.0f;
			for(uint32_t sample = 0; sample < MAGNETOMETER_BASELINE_SAMPLE_COUNT; sample++)
			{
				sum += baseline_samples[sample][magnetometer][axis];
			}
			magnetometer_baseline[magnetometer][axis] =
				sum / (float)MAGNETOMETER_BASELINE_SAMPLE_COUNT;
			consecutive_deviation_count[magnetometer][axis] = 0u;
		}
	}

	magnetometer_baseline_valid = true;
	return true;
}

void monitor_magnetometer_readings(void)
{
	if(system_status[SS_STATE].u != STATE_EMISSION)
	{
		collect_baseline_sample();
		return;
	}

	if(!magnetometer_baseline_valid)
	{
		return;
	}

	for(uint32_t magnetometer = 0; magnetometer < MAGNETOMETER_COUNT; magnetometer++)
	{
		for(uint32_t axis = 0; axis < MAGNETOMETER_AXIS_COUNT; axis++)
		{
			float baseline = magnetometer_baseline[magnetometer][axis];
			float reading = system_status[magnetometer_status_fields[magnetometer][axis]].f;
			float deviation = fabsf(reading - baseline);
			float normal_threshold = allowed_deviation(
				baseline,
				MAGNETOMETER_DEVIATION_TOLERANCE_PERCENT,
				MAGNETOMETER_MINIMUM_DEVIATION_UT);

			if(!isfinite(reading))
			{
				magnetometer_baseline_valid = false;
				report_typed_fault2(
					FAULT_MAGNETOMETER,
					"Magnetometer %u axis %u reported a non-finite reading during emission.",
					MAKE_ARG(magnetometer + 1u),
					MAKE_ARG(axis + 1u));
				return;
			}

			if(deviation > normal_threshold)
			{
				consecutive_deviation_count[magnetometer][axis]++;
				if(consecutive_deviation_count[magnetometer][axis] >=
					MAGNETOMETER_CONSECUTIVE_DEVIATION_COUNT)
				{
					report_deviation_fault(magnetometer, axis, reading, baseline, normal_threshold);
					return;
				}
			}
			else
			{
				consecutive_deviation_count[magnetometer][axis] = 0u;
			}
		}
	}
}
