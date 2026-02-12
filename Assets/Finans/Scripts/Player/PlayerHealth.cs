using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages player health system with dynamic damage and healing based on distance and fruit collection.
/// 
/// HEALTH SYSTEM LOGIC:
/// - Health starts at 100% (maxHealth)
/// - Decreases automatically every 3 seconds based on distance calculation
/// - Increases when fruits are collected (dynamic value based on total fruits)
/// - Player dies when health reaches 0%
/// - Health is clamped between 0 and maxHealth (100%)
/// </summary>
public class PlayerHealth : MonoBehaviour
{
	[SerializeField]
	private int maxHealth = 100; // Maximum health value (100 = 100%)

	[SerializeField]
	private bool useFallbackDamageTimer = true; // Enable automatic time-based damage

	[SerializeField, Range(0f, 100f)]
	public float damagePercentPerTick = 5f; // Damage percentage applied every 3 seconds (auto-calculated from distance)

	[SerializeField, Range(0f, 100f)]
	public float healPercentPerFruit = 10f; // Healing percentage per fruit (auto-calculated: 100% / fruitThreshold)
	
	[Header("Distance-Based Damage")]
	[Tooltip("Total time in seconds to travel from Penguin to Castle")]
	public float totalTravelTime = 30f;
	
	[SerializeField, Range(0f, 1f)]
	[Tooltip("Percentage of total travel time for health to reach 0% (0.75 = 75%)")]
	private float damageTimePercentage = 0.75f;
	
	[Header("Optional: Assign in Inspector for Better Performance")]
	[Tooltip("Optional: Assign Penguin GameObject here to avoid searching. Leave empty to auto-find.")]
	[SerializeField]
	private GameObject penguinObject;
	
	[Tooltip("Optional: Assign Castle GameObject here to avoid searching. Leave empty to auto-find.")]
	[SerializeField]
	private GameObject castleObject;
	
	public event Action<float> OnHealthChanged; // 0..1
	public event Action<int, int, int> OnFruitCountChanged; // collected, total, threshold

	private int totalFruits;
	private int fruitThreshold;
	private int fruitsCollected;
	private float currentHealth;
	private float calculatedDistance = 0f;

	public float HealthPercent => Mathf.Approximately(maxHealth, 0) ? 0f : currentHealth / maxHealth;

	private void Start()
	{
		// Initialize health to maximum (100%)
		currentHealth = maxHealth;
		
		// FRUIT SYSTEM: Count all fruits in scene and calculate dynamic healing value
		// Formula: fruitThreshold = Ceil(75% of total fruits)
		//          healPercentPerFruit = 100% / fruitThreshold
		// Example: 4 fruits → threshold = 3 → each fruit = 33.33%
		CountFruitsAndThreshold();
		
		// DISTANCE-BASED DAMAGE: Calculate damage rate based on Penguin to Castle distance
		// Formula: damagePercentPerTick = (100% / (totalTravelTime × 0.75)) × 3 seconds
		// This ensures health reaches 0% in 75% of total travel time
		CalculateDistanceAndDamageRate();
		
		// Initialize UI and events
		RaiseHealthChanged(0f);
		RaiseFruitCountChanged();

		// Start automatic damage coroutine (damages every 3 seconds)
		if (useFallbackDamageTimer)
		{
			StartCoroutine(DamageTickCoroutine());
		}
	}

	/// <summary>
	/// DECREASING HEALTH LOGIC: Applies damage to player health.
	/// - Clamps health between 0 and maxHealth
	/// - Triggers death (GameOver) when health reaches 0%
	/// - Notifies UI via OnHealthChanged event
	/// </summary>
	public void ApplyDamage(float amount)
	{
		if (amount <= 0f) return;
		currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
		RaiseHealthChanged(0f);
		
		// DEATH LOGIC: Check if health reached 0%
		if (Mathf.Approximately(currentHealth, 0f))
		{
			// Safety check for GameManager instance
			if (GameManager.instance != null)
			{
				GameManager.instance.GameOver();
			}
			else
			{
				Debug.LogError("PlayerHealth: GameManager.instance is null. Cannot call GameOver().");
			}
			//Logger.LogInfo($"FS Life left now is : {PointSystem.Life-1}", "PlayerHealth");
			//Logger.LogInfo($"Current health is : {currentHealth}", "PlayerHealth");
		}
	}

	public void ApplyDamagePercent(float percent)
	{
		if (percent <= 0f) return;
		var amount = (percent / 100f) * maxHealth;
		ApplyDamage(amount);
	}

	/// <summary>
	/// INCREASING HEALTH LOGIC: Heals player health.
	/// - Clamps health at maxHealth (100%) - prevents overflow
	/// - Notifies UI via OnHealthChanged event
	/// </summary>
	public void Heal(float amount)
	{
		if (amount <= 0f) return;
		currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
		RaiseHealthChanged(amount);
	}

	/// <summary>
	/// Heals player by percentage (converts to absolute amount).
	/// </summary>
	public void HealPercent(float percent)
	{
		if (percent <= 0f) return;
		var amount = (percent / 100f) * maxHealth;
		Heal(amount);

	}

	/// <summary>
	/// FRUIT COLLECTION LOGIC: Called when player collects a fruit.
	/// - Increments fruit count
	/// - Heals player by dynamic fruit value (healPercentPerFruit)
	/// - Each fruit value = 100% / fruitThreshold (ensures threshold fruits = 100% health)
	/// - Health is clamped at 100% (cannot exceed maximum)
	/// </summary>
	public void NotifyFruitCollected()
	{
		fruitsCollected = Mathf.Clamp(fruitsCollected + 1, 0, totalFruits);
		//Logger.LogInfo($":{fruitsCollected} fruit collected, total fruits are {totalFruits}", "PlayerHealth");
		HealPercent(healPercentPerFruit);
		RaiseFruitCountChanged();
	}

	/// <summary>
	/// FRUIT POWER CALCULATION: Scans scene for all fruits and calculates dynamic healing value.
	/// 
	/// LOGIC:
	/// 1. Find all GameObjects tagged "Fruit" in scene
	/// 2. Calculate fruitThreshold = Ceil(75% of total fruits)
	///    - This is the number of fruits needed to reach 100% health
	/// 3. Calculate healPercentPerFruit = 100% / fruitThreshold
	///    - Each fruit heals by this percentage
	///    - Collecting threshold fruits = 100% health total
	/// 
	/// EXAMPLE:
	/// - 4 fruits total → threshold = 3 → each fruit = 33.33%
	/// - 10 fruits total → threshold = 8 → each fruit = 12.5%
	/// - 1 fruit total → threshold = 1 → each fruit = 100%
	/// </summary>
	private void CountFruitsAndThreshold()
	{
		var fruits = GameObject.FindGameObjectsWithTag("Fruit");
		totalFruits = fruits?.Length ?? 0;
		fruitThreshold = Mathf.CeilToInt(totalFruits * 0.75f);
		
		// Calculate dynamic fruit value: 100% / fruitThreshold
		// Each fruit collected will heal by this percentage
		if (fruitThreshold > 0)
		{
			healPercentPerFruit = 100f / fruitThreshold;
		}
		else
		{
			// Fallback if no fruits found
			healPercentPerFruit = 10f;
		}
	}

	/// <summary>
	/// DISTANCE-BASED DAMAGE CALCULATION: Calculates automatic damage rate based on level distance.
	/// Uses coroutine to prevent main thread blocking.
	/// </summary>
	private void CalculateDistanceAndDamageRate()
	{
		// Use coroutine to find objects asynchronously to prevent hangs
		// This prevents blocking the main thread
		StartCoroutine(CalculateDistanceAndDamageRateCoroutine());
	}
	
	/// <summary>
	/// DISTANCE-BASED DAMAGE LOGIC: Finds Penguin and Castle, calculates distance, sets damage rate.
	/// 
	/// FORMULA:
	/// 1. Distance = |Castle X - Penguin X|
	/// 2. Damage Time = totalTravelTime × 0.75 (75% of total time)
	/// 3. Damage Per Second = 100% / Damage Time
	/// 4. Damage Per Tick = Damage Per Second × 3 seconds
	/// 
	/// EXAMPLE:
	/// - Distance: 95.5 units, Total Time: 30s
	/// - Damage Time: 30 × 0.75 = 22.5s
	/// - Damage Per Second: 100% / 22.5 = 4.44%/s
	/// - Damage Per Tick: 4.44% × 3 = 13.33% per tick
	/// 
	/// RESULT: Health decreases from 100% to 0% in 75% of total travel time.
	/// </summary>
	private IEnumerator CalculateDistanceAndDamageRateCoroutine()
	{
		// Yield one frame to ensure scene is fully loaded
		yield return null;
		
		GameObject penguin = penguinObject;
		GameObject castle = castleObject;
		
		// If not assigned in inspector, try to find them
		if (penguin == null || castle == null)
		{
			// Use GameObject.Find only if not assigned - this is expensive but necessary for auto-find
			if (penguin == null)
			{
				try
				{
					penguin = GameObject.Find("2. Little Penguin");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"PlayerHealth: Error finding Penguin: {e.Message}");
				}
			}
			
			if (castle == null)
			{
				try
				{
					castle = GameObject.Find("4. Castle");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"PlayerHealth: Error finding Castle: {e.Message}");
				}
			}
		}
		
		if (penguin != null && castle != null)
		{
			// Calculate distance using X position
			calculatedDistance = Mathf.Abs(castle.transform.position.x - penguin.transform.position.x);
			
			// Calculate damage rate: 100% damage in 75% of total travel time
			float damageTime = totalTravelTime * damageTimePercentage;
			
			// Safety check: prevent division by zero
			if (damageTime > 0.01f)
			{
				// Calculate damage per second
				float damagePerSecond = 100f / damageTime;
				
				// Calculate damage per tick (currently 3 seconds per tick)
				float tickInterval = 3f;
				damagePercentPerTick = damagePerSecond * tickInterval;
				
				// Clamp to reasonable values (0-100%)
				damagePercentPerTick = Mathf.Clamp(damagePercentPerTick, 0f, 100f);
			}
			else
			{
				Debug.LogWarning("PlayerHealth: Invalid damageTime calculated. Using default damage rate.");
			}
		}
		else
		{
			// Fallback if objects not found - use default value
			if (penguin == null)
				Debug.LogWarning("PlayerHealth: Could not find '2. Little Penguin' game object. Using default damage rate.");
			if (castle == null)
				Debug.LogWarning("PlayerHealth: Could not find '4. Castle' game object. Using default damage rate.");
		}
	}

	private void RaiseHealthChanged(float amount)
	{
		OnHealthChanged?.Invoke(HealthPercent);
		//Logger.LogInfo($"Health percent changed : {HealthPercent}", "PlayerHealth");
		//Logger.LogInfo($"Health healed by: {amount}%, current health is {currentHealth}", "PlayerHealth");
	}

	private void RaiseFruitCountChanged()
	{
		OnFruitCountChanged?.Invoke(fruitsCollected, totalFruits, fruitThreshold);
		//Logger.LogInfo($"Fruit count increased to  {fruitsCollected}", "PlayerHealth");
	}

	/// <summary>
	/// AUTOMATIC DAMAGE COROUTINE: Applies damage every 3 seconds while game is playing.
	/// - Damage amount is calculated from distance (damagePercentPerTick)
	/// - Only damages when game state is "Playing"
	/// - Continues until player dies or game ends
	/// </summary>
	private IEnumerator DamageTickCoroutine()
	{
		var wait = new WaitForSeconds(3f);
		while (true)
		{
			yield return wait;
			// Safety check for GameManager
			if (GameManager.instance != null && GameManager.CurrentState == GameManager.GameState.Playing)
			{
				//Logger.LogInfo($"3 second damage of 5% occured : {damagePercentPerTick}", "PlayerHealth");
				ApplyDamagePercent(damagePercentPerTick);
			}
		}
	}
}


