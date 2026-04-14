//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Connector between engine and the gearbox. Transmits the received power from the engine to the gearbox based on clutch input.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Drivetrain/RCCP Clutch")]
public class RCCP_Clutch : RCCP_Component {

    /// <summary>
    /// If true, overrides all calculations and uses a custom clutch input set externally.
    /// </summary>
    public bool overrideClutch = false;

    /// <summary>
    /// Current clutch input, clamped between 0 and 1. 
    /// - 0 means fully engaged clutch (no slip), 
    /// - 1 means clutch is fully pressed (full slip).
    /// </summary>
    [Range(0f, 1f)] public float clutchInput = 1f;

    /// <summary>
    /// Raw input used to gradually reach the actual clutch input. 
    /// Useful for smoothing transitions when using an automatic clutch.
    /// </summary>
    [Range(0f, 1f)] private float clutchInputRaw = 1f;

    /// <summary>
    /// How quickly the clutch input changes (higher inertia = slower changes). Only used when automaticClutch is true.
    /// </summary>
    [Range(0f, 1f)] public float clutchInertia = .2f;

    /// <summary>
    /// If true, clutch input is automatically calculated. If false, clutchInput_P from the player is used directly.
    /// </summary>
    public bool automaticClutch = true;

    /// <summary>
    /// If true, forces the clutch input to 1 (fully pressed).
    /// </summary>
    public bool forceToPressClutch = false;

    /// <summary>
    /// If true, clutch input is forced to 1 (fully pressed) while shifting gears.
    /// </summary>
    public bool pressClutchWhileShiftingGears = true;

    /// <summary>
    /// If true, clutch input is forced to 1 (fully pressed) while the handbrake is applied.
    /// </summary>
    public bool pressClutchWhileHandbraking = true;

    /// <summary>
    /// If engine RPM falls below this value, the clutch input will be increased to avoid stalling (when automaticClutch is true).
    /// </summary>
    [Min(0f)] public float engageRPM = 1200f;

    /// <summary>
    /// RPM threshold above engageRPM where clutch starts to engage smoothly when no throttle is applied.
    /// </summary>
    [Min(0f)] private float engageRPMBuffer = 600f;

    /// <summary>
    /// Minimum speed threshold where clutch disengages regardless of RPM when coasting.
    /// </summary>
    [Min(0f)] private float coastingSpeedThreshold = 20f;

    /// <summary>
    /// How aggressively the clutch system responds to rapid RPM changes. Higher values make it more predictive.
    /// </summary>
    [Range(0f, 2f)] private float predictiveSensitivity = 1f;

    /// <summary>
    /// Hysteresis value to prevent oscillation when RPM is near engage threshold.
    /// </summary>
    [Range(0f, 500f)] private float hysteresisBuffer = 2000f;

    /// <summary>
    /// Torque received from the previous component (usually the engine).
    /// </summary>
    public float receivedTorqueAsNM = 0f;

    /// <summary>
    /// Torque delivered to the next component (usually the gearbox).
    /// </summary>
    public float producedTorqueAsNM = 0f;

    /// <summary>
    /// Output event with a custom output class, holding the torque.
    /// </summary>
    public RCCP_Event_Output outputEvent = new RCCP_Event_Output();
    public RCCP_Output output = new RCCP_Output();

    // Private variables for smooth operation
    float velocity;
    float previousEngineRPM;
    float rpmChangeRate;
    float rpmChangeRateSmoothed;
    float rpmVelocity;
    float predictedRPM;
    bool clutchEngaged = false;
    float clutchEngagementTimer = 0f;
    float targetClutchInput = 1f;
    float clutchSmoothingFactor = 0.1f;

    private void FixedUpdate() {

        // Calculating clutch input based on engine RPM, speed, etc. (or player input if automaticClutch is false).
        Input();

        // Delivering torque to the gearbox or other connected component.
        Output();

    }

    /// <summary>
    /// Calculates the clutch input based on current settings.
    /// </summary>
    private void Input() {

        // If overrideClutch is true, the input is set externally; skip internal calculations.
        if (overrideClutch)
            return;

        // Automatic clutch logic.
        if (automaticClutch) {

            // Check if we have valid CarController reference.
            if (CarController == null)
                return;

            // Get current vehicle state.
            float currentEngineRPM = CarController.engineRPM;
            float currentSpeed = CarController.absoluteSpeed;
            float throttleInput = CarController.throttleInput_V;
            bool isShifting = CarController.shiftingNow;
            bool isHandbraking = CarController.handbrakeInput_V >= .75f;
            bool isInNeutral = CarController.NGearNow;
            bool isReversing = CarController.reversingNow;
            float brakeInput = CarController.brakeInput_V;

            // Calculate RPM change rate for predictive behavior.
            if (Time.fixedDeltaTime > 0) {

                rpmChangeRate = (currentEngineRPM - previousEngineRPM) / Time.fixedDeltaTime;
                rpmChangeRateSmoothed = Mathf.SmoothDamp(rpmChangeRateSmoothed, rpmChangeRate, ref rpmVelocity, 0.1f);

            }

            // Predict future RPM based on current trend.
            float predictionTime = 0.6f; // Look ahead 0.3 seconds.
            predictedRPM = currentEngineRPM + (rpmChangeRateSmoothed * predictionTime * predictiveSensitivity);

            // Store current RPM for next frame.
            previousEngineRPM = currentEngineRPM;

            // Determine engagement thresholds with hysteresis.
            float engageThreshold = clutchEngaged ? (engageRPM - hysteresisBuffer) : engageRPM;
            float disengageThreshold = clutchEngaged ? engageRPM : (engageRPM + hysteresisBuffer);

            // Primary logic for automatic clutch engagement.
            float baseClutchInput = 1f; // Default to pressed (disengaged).

            // Check if we should engage or disengage the clutch.
            if (predictedRPM > engageThreshold || currentEngineRPM > engageThreshold) {

                // RPM is sufficient or predicted to be sufficient.
                if (throttleInput >= .1f) {

                    // Player is applying throttle.
                    if (currentSpeed < 2f) {

                        // Starting from standstill - gradual engagement based on RPM.
                        float rpmRatio = Mathf.Clamp01((currentEngineRPM - engageRPM) / engageRPMBuffer);
                        float throttleRatio = Mathf.Clamp01(throttleInput * 2f);
                        baseClutchInput = 1f - (rpmRatio * throttleRatio);

                        // If RPM is rising quickly, engage faster.
                        if (rpmChangeRateSmoothed > 500f) {

                            float rapidEngageFactor = Mathf.Clamp01(rpmChangeRateSmoothed / 2000f);
                            baseClutchInput *= (1f - rapidEngageFactor * 0.5f);

                        }

                    } else {

                        // Vehicle is moving, engage clutch fully.
                        baseClutchInput = 0f;

                    }

                    clutchEngaged = true;

                } else if (currentSpeed > coastingSpeedThreshold) {

                    // Vehicle is moving and coasting without throttle.
                    // Keep clutch engaged to allow engine braking.
                    baseClutchInput = 0f;
                    clutchEngaged = true;

                } else if (brakeInput > 0.1f && currentSpeed > 1f) {

                    // Braking while moving - keep engaged for engine braking.
                    baseClutchInput = 0f;
                    clutchEngaged = true;

                } else {

                    // Low speed, no throttle.
                    if (currentEngineRPM > (engageRPM + engageRPMBuffer)) {

                        // RPM is well above engage threshold, keep engaged.
                        baseClutchInput = 0f;
                        clutchEngaged = true;

                    } else if (currentEngineRPM < disengageThreshold) {

                        // RPM approaching stall threshold, disengage smoothly.
                        float disengageRatio = Mathf.Clamp01((currentEngineRPM - (engageRPM - 200f)) / 400f);
                        baseClutchInput = 1f - disengageRatio;

                        if (baseClutchInput > 0.9f) {

                            clutchEngaged = false;

                        }

                    }

                }

            } else {

                // Engine RPM is below engage threshold.
                if (rpmChangeRateSmoothed > 1000f && throttleInput > 0.5f) {

                    // RPM rising rapidly with high throttle - start pre-engaging.
                    float preEngageRatio = Mathf.Clamp01((predictedRPM - (engageRPM * 0.8f)) / (engageRPM * 0.2f));
                    baseClutchInput = 1f - (preEngageRatio * 0.3f);

                } else {

                    // Keep disengaged to prevent stalling.
                    baseClutchInput = 1f;
                    clutchEngaged = false;

                }

            }

            // Special cases that override normal logic.

            // Force clutch disengagement when in neutral gear.
            if (isInNeutral) {

                baseClutchInput = 1f;
                clutchEngaged = false;

            }

            // Force clutch disengagement if gearbox is shifting.
            if (pressClutchWhileShiftingGears && isShifting) {

                baseClutchInput = 1f;
                clutchEngagementTimer = 0f;

            }

            // Force clutch disengagement if handbrake is applied strongly.
            if (pressClutchWhileHandbraking && isHandbraking) {

                baseClutchInput = 1f;
                clutchEngaged = false;

            }

            // Smooth engagement timer to prevent abrupt changes after shifting.
            if (!isShifting && clutchEngagementTimer < 1f) {

                clutchEngagementTimer += Time.fixedDeltaTime * 2f;
                clutchEngagementTimer = Mathf.Clamp01(clutchEngagementTimer);
                baseClutchInput = Mathf.Lerp(1f, baseClutchInput, clutchEngagementTimer);

            }

            float adjustedClutchIntertia = Mathf.Lerp(clutchInertia * 5f, clutchInertia, Mathf.InverseLerp(0f, coastingSpeedThreshold, CarController.absoluteSpeed));
            adjustedClutchIntertia = Mathf.Clamp01(adjustedClutchIntertia);

            // Apply smoothing based on the situation.
            if (Mathf.Abs(baseClutchInput - targetClutchInput) > 0.8f) {

                // Large change - use slower smoothing.
                clutchSmoothingFactor = adjustedClutchIntertia * 0.5f;

            } else {

                // Small change - use faster smoothing.
                clutchSmoothingFactor = adjustedClutchIntertia * 0.2f;

            }

            if (CarController.absoluteSpeed < (coastingSpeedThreshold * .5f) && throttleInput < .1f)
                baseClutchInput = 1f;

            // Update target and smooth the actual input.
            targetClutchInput = baseClutchInput;
            clutchInputRaw = Mathf.Clamp01(targetClutchInput);
            clutchInput = Mathf.SmoothDamp(clutchInput, clutchInputRaw, ref velocity, clutchSmoothingFactor);

            // Prevent micro-oscillations near extremes.
            if (clutchInputRaw > .98f && clutchInput > .95f)
                clutchInput = 1f;

            if (clutchInputRaw < .02f && clutchInput < .05f)
                clutchInput = 0f;

        } else {

            // If automatic clutch is disabled, get the clutch input directly from the player's input.
            clutchInput = CarController.clutchInput_P;

        }

        // If forced, override the clutch input to 1 (fully pressed).
        if (forceToPressClutch)
            clutchInput = 1f;

        // Ensure final clutch input is within [0,1].
        clutchInput = Mathf.Clamp01(clutchInput);

    }

    /// <summary>
    /// Overrides the internally calculated clutch input with a specific value.
    /// </summary>
    /// <param name="targetInput">Value between 0 and 1 (0 = fully engaged, 1 = fully pressed)</param>
    public void OverrideInput(float targetInput) {

        clutchInput = targetInput;
        clutchInputRaw = targetInput;
        targetClutchInput = targetInput;

    }

    /// <summary>
    /// Called by the previous component in the drivetrain to deliver torque into this clutch.
    /// </summary>
    /// <param name="output"></param>
    public void ReceiveOutput(RCCP_Output output) {

        receivedTorqueAsNM = output.NM;

    }

    /// <summary>
    /// Outputs the torque after applying the clutch slip factor.
    /// </summary>
    private void Output() {

        if (output == null)
            output = new RCCP_Output();

        // If clutch is fully pressed (1), torque is near 0. If not pressed (0), full torque is passed through.
        producedTorqueAsNM = receivedTorqueAsNM * (1f - clutchInput);

        output.NM = producedTorqueAsNM;
        outputEvent.Invoke(output);

    }

    /// <summary>
    /// Resets essential clutch variables to their default states.
    /// </summary>
    public void Reload() {

        clutchInput = 1f;
        clutchInputRaw = 1f;
        targetClutchInput = 1f;
        receivedTorqueAsNM = 0f;
        producedTorqueAsNM = 0f;
        previousEngineRPM = 0f;
        rpmChangeRate = 0f;
        rpmChangeRateSmoothed = 0f;
        predictedRPM = 0f;
        clutchEngaged = false;
        clutchEngagementTimer = 0f;
        velocity = 0f;
        rpmVelocity = 0f;

    }

}