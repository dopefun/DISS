using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPulseNoise : MonoBehaviour {

    public Rigidbody drone;

    public bool apply_force = true;

    public float strength_coef = 0.0015f;
    public float strength_mean = 30.0f;
    public float strength_variance = 20.0f;
    public float strength_hold_variance = 1000.0f;
    public float pulse_period_mean = 7;
    public float pulse_period_variance = 5;
    public float pulse_duration_mean = 10;
    public float pulse_duration_variance = 2;
    public float motion_period_mean = 8;
    public float motion_period_variance = 3f;
    public float wind_change_speed_mean = 0.05f;
    public float wind_change_speed_variance = 0.01f;

    public float strength_off_speed = 50.0f;
    public float strength_on_speed = 70.0f;
    System.Random r;
    float pulse_timer = 0.0f;
    float pulse_period = 0.0f;
    float pulse_duration = 0.0f;
    float base_strength = 0.0f;
    float strength = 0.0f;
    int pulse_mode = 0;
    float motion_timer = 0.0f;
    float motion_period = 0.0f;
    float wind_change_speed = 0.0f;
    public Quaternion targetDirection;
    int motion_mode = 0;

	void Start () {
        r = new System.Random();
	}

	void FixedUpdate () 
    {
        if (pulse_mode == 0)
        {
            pulse_timer = 0.0f;
            pulse_period = SamplePositive(pulse_period_mean, pulse_period_variance);
            pulse_mode = 1;
        } 
        else if (pulse_mode == 1) 
        {
            pulse_timer += Time.deltaTime;
            strength = strength - Time.deltaTime * strength_off_speed;
            if (strength < 0.0f) 
            {
                strength = 0.0f;
            }
            if (pulse_timer >= pulse_period)
            {
                pulse_timer = 0.0f; //reset
                pulse_duration = SamplePositive(pulse_duration_mean, pulse_duration_variance);
                base_strength = SamplePositive(strength_mean, strength_variance);
                pulse_mode = 2;
            }
        } 
        else if (pulse_mode == 2)
        {
            pulse_timer += Time.deltaTime;
            if (pulse_timer >= pulse_duration) 
            {
                pulse_timer = 0.0f;
                pulse_mode = 0;
            } 
            else 
            {
                float target_strength = Sample(base_strength, strength_hold_variance);

                if (Mathf.Abs(strength - target_strength) / (target_strength + 1e-8) < 0.4)
                {
                    strength = target_strength;
                }
                else
                {
                    int dir = target_strength > strength ? 1 : -1;
                    strength = strength + Time.deltaTime * strength_on_speed;

                    if (dir * strength > dir * target_strength)
                    {
                        strength = target_strength;
                    }
                }
            }
        } 

        if (motion_mode == 0) 
        {
            motion_timer = 0.0f;
            motion_period = SamplePositive(motion_period_mean, motion_period_variance);
            wind_change_speed = SamplePositive(wind_change_speed_mean, wind_change_speed_variance);
            targetDirection = Quaternion.Euler(new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f));
            motion_mode = 1;
        }
        else if (motion_mode == 1)
        {
            motion_timer += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * wind_change_speed);
            if (motion_timer > motion_period) {
                motion_timer = 0.0f;
                motion_mode = 0; 

            }
        }

        Vector3 ray = strength * (transform.rotation * Vector3.forward);

        if (apply_force)
        {
            drone.AddForce(ray * strength_coef, ForceMode.Impulse);
        }
        Debug.DrawRay(drone.position, ray, Color.green);
	}

    public float Sample(float mean, float var)
    {
        float n = NextGaussianDouble();

        return n * Mathf.Sqrt(var) + mean;
    }

    public float SamplePositive(float mean, float var) {
        return Mathf.Abs(Sample(mean, var));
    }

    public float NextGaussianDouble()
    {
        float u, v, S;

        do
        {
            u = 2.0f * (float) r.NextDouble() - 1.0f;
            v = 2.0f * (float) r.NextDouble() - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }
}