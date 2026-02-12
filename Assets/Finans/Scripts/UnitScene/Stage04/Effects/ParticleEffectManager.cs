using UnityEngine;

/// <summary>
/// Helper class to create and manage particle effects
/// </summary>
public static class ParticleEffectManager
{
    public static ParticleSystem CreateCorrectEffect()
    {
        GameObject effectObj = new GameObject("CorrectEffect");
        ParticleSystem particleSystem = effectObj.AddComponent<ParticleSystem>();
        
        // Main module
        var main = particleSystem.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.startColor = new Color(0.2f, 1f, 0.2f); // Green
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 50;

        // Emission module
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));
        emission.SetBurst(1, new ParticleSystem.Burst(0.3f, 20));

        // Shape module
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        shape.arc = 360f;

        // Color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.green, 0.0f), 
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.3f);
        curve.AddKey(0.2f, 1.5f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Velocity over lifetime
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);

        // Renderer settings
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.sortingOrder = 100; // Ensure it renders on top

        return particleSystem;
    }

    public static ParticleSystem CreateWrongEffect()
    {
        GameObject effectObj = new GameObject("WrongEffect");
        ParticleSystem particleSystem = effectObj.AddComponent<ParticleSystem>();
        
        // Main module
        var main = particleSystem.main;
        main.duration = 2.5f;
        main.loop = false;
        main.startLifetime = 2f;
        main.startSpeed = 8f;
        main.startSize = 0.8f;
        main.startColor = new Color(1f, 0.1f, 0.1f); // Bright red
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 80;

        // Emission module
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 50));
        emission.SetBurst(1, new ParticleSystem.Burst(0.2f, 30));
        emission.SetBurst(2, new ParticleSystem.Burst(0.5f, 20));

        // Shape module
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.5f;
        shape.arc = 360f;

        // Color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f), 
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.3f), // Orange
                new GradientColorKey(Color.yellow, 0.7f),
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(0.8f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.1f);
        curve.AddKey(0.1f, 2.5f);
        curve.AddKey(0.5f, 1.5f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Velocity over lifetime
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(6f);

        // Noise module for more dynamic movement
        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(1.2f);
        noise.frequency = 1f;
        noise.scrollSpeed = 2f;

        // Renderer settings
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.sortingOrder = 100; // Ensure it renders on top

        return particleSystem;
    }

    public static ParticleSystem CreatePickupEffect()
    {
        GameObject effectObj = new GameObject("PickupEffect");
        ParticleSystem particleSystem = effectObj.AddComponent<ParticleSystem>();
        
        // Main module
        var main = particleSystem.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.5f, 0.8f, 1f); // Light blue
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 20;

        // Emission module
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

        // Shape module
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.8f;
        shape.arc = 360f;

        // Color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.cyan, 0.0f), 
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.5f);
        curve.AddKey(0.5f, 1.2f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Renderer settings
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.sortingOrder = 100; // Ensure it renders on top

        return particleSystem;
    }

    /// <summary>
    /// Creates and plays a VFX effect at the specified world position
    /// </summary>
    public static void PlayVFXAtPosition(ParticleSystem effectPrefab, Vector3 worldPosition, Transform parent = null)
    {
        if (effectPrefab == null) return;

        // Create the effect
        var effect = Object.Instantiate(effectPrefab, worldPosition, Quaternion.identity);
        
        // Set parent if specified, otherwise use root
        if (parent != null)
        {
            effect.transform.SetParent(parent);
        }
        else
        {
            effect.transform.SetParent(null); // Root level
        }
        
        // Play the effect
        effect.Play();
        
        // Destroy after completion
        float destroyDelay = effect.main.duration + 1f; // Extra buffer
        Object.Destroy(effect.gameObject, destroyDelay);
        
        Debug.Log($"VFX played at position: {worldPosition}");
    }

    /// <summary>
    /// Creates and plays a correct effect at the specified world position
    /// </summary>
    public static void PlayCorrectVFXAtPosition(Vector3 worldPosition, Transform parent = null)
    {
        var effect = CreateCorrectEffect();
        effect.transform.position = worldPosition;
        
        if (parent != null)
        {
            effect.transform.SetParent(parent);
        }
        else
        {
            effect.transform.SetParent(null);
        }
        
        effect.Play();
        Object.Destroy(effect.gameObject, effect.main.duration + 1f);
        
        Debug.Log($"Correct VFX played at position: {worldPosition}");
    }

    /// <summary>
    /// Creates and plays a wrong effect at the specified world position
    /// </summary>
    public static void PlayWrongVFXAtPosition(Vector3 worldPosition, Transform parent = null)
    {
        var effect = CreateWrongEffect();
        effect.transform.position = worldPosition;
        
        if (parent != null)
        {
            effect.transform.SetParent(parent);
        }
        else
        {
            effect.transform.SetParent(null);
        }
        
        effect.Play();
        Object.Destroy(effect.gameObject, effect.main.duration + 1f);
        
        Debug.Log($"Wrong VFX played at position: {worldPosition}");
    }
} 