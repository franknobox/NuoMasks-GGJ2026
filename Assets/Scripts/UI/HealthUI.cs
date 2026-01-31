using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerGo player;
    [SerializeField] private Transform container;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    private List<Image> hearts = new List<Image>();
    private int lastHealth;

    private void OnEnable()
    {
        if (player == null || container == null || heartPrefab == null)
        {
            Debug.LogError("HealthUI: player、container 或 heartPrefab 未赋值，已禁用。");
            enabled = false;
            return;
        }
        player.OnHealthChanged += OnHealthChanged;
        lastHealth = player.CurrentHealth;
        UpdateHearts(player.CurrentHealth, player.MaxHealth);
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < lastHealth)
        {
            for (int i = current; i < lastHealth; i++)
            {
                if (i < hearts.Count)
                    ApplyLoseHeart(hearts[i]);
            }
        }
        if (current > lastHealth)
        {
            for (int i = lastHealth; i < current; i++)
            {
                if (i < hearts.Count)
                    ApplyGainHeart(hearts[i]);
            }
        }
        lastHealth = current;
        RefreshVisibility(current, max);
    }

    private void UpdateHearts(int current, int max)
    {
        if (hearts.Count != max)
        {
            StopAllCoroutines();
            foreach (Transform child in container)
                Destroy(child.gameObject);
            hearts.Clear();
            for (int i = 0; i < max; i++)
            {
                Image heart = Instantiate(heartPrefab, container);
                if (fullSprite != null)
                    heart.sprite = fullSprite;
                hearts.Add(heart);
            }
        }

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].gameObject.SetActive(i < current);
    }

    private void RefreshVisibility(int current, int max)
    {
        if (hearts.Count != max) return;
        for (int i = 0; i < hearts.Count; i++)
        {
            bool shouldShow = i < current;
            if (hearts[i].gameObject.activeSelf != shouldShow)
                hearts[i].gameObject.SetActive(shouldShow);
        }
    }

    private void ApplyLoseHeart(Image heart)
    {
        if (heart == null) return;
        Animator anim = heart.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Pop");
            StartCoroutine(DisableAfterDelay(heart.gameObject, 0.2f));
        }
        else
        {
            StartCoroutine(LoseHeartScaleRoutine(heart));
        }
    }

    private IEnumerator LoseHeartScaleRoutine(Image heart)
    {
        if (heart == null) yield break;
        RectTransform rt = heart.rectTransform;
        float elapsed = 0f;
        const float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.localScale = Vector3.one * (1f - t);
            yield return null;
        }
        if (heart != null)
            heart.gameObject.SetActive(false);
    }

    private IEnumerator DisableAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null)
            go.SetActive(false);
    }

    private void ApplyGainHeart(Image heart)
    {
        if (heart == null) return;
        heart.gameObject.SetActive(true);
        RectTransform rt = heart.rectTransform;
        if (rt != null) rt.localScale = Vector3.one;
        if (heart.color.a != 1f)
        {
            Color c = heart.color;
            c.a = 1f;
            heart.color = c;
        }
    }
}
