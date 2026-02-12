using UnityEngine;

namespace Game.Core
{
    public sealed class VFXService : MonoBehaviour
    {
        public static VFXService Instance { get; private set; }

		[SerializeField] private ParticleSystem correctBurstPrefab;
		[SerializeField] private ParticleSystem wrongBurstPrefab;
        [SerializeField] private float wrongShakeAmount = 6f;
        [SerializeField] private float wrongShakeDuration = 0.2f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SpawnCorrectBurst(Vector3 position)
        {
            if (correctBurstPrefab == null) return;
            var ps = Instantiate(correctBurstPrefab, position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

		public void SpawnWrongBurst(Vector3 position)
		{
			if (wrongBurstPrefab == null) return;
			var ps = Instantiate(wrongBurstPrefab, position, Quaternion.identity);
			ps.Play();
			Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
		}

        public void ShakeOnWrong(Transform target)
        {
            if (target == null) return;
            StopAllCoroutines();
            StartCoroutine(ShakeCoroutine(target));
        }

        private System.Collections.IEnumerator ShakeCoroutine(Transform target)
        {
            var original = target.localPosition;
            float elapsed = 0f;
            while (elapsed < wrongShakeDuration)
            {
                float x = Random.Range(-wrongShakeAmount, wrongShakeAmount) * 0.01f;
                float y = Random.Range(-wrongShakeAmount, wrongShakeAmount) * 0.01f;
                target.localPosition = original + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.localPosition = original;
        }
    }
}


