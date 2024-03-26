using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthStaminaSystem : MonoBehaviour
{
    #region Health Stamina Panel
    public float panelFadeOutInterval;
    public float panelFadeOutSpeed;
    float lastHealthStaminaUsedTime;
    bool isPanelActive;
    bool fadePanelNow;
    public bool playerInCombatMode = true;
    public CanvasGroup panelUIRenderer;
    public Image healthBar, staminaBar;
    #endregion

    #region Character Combat Properties
    public bool isPlayer, isBoss, playerInsideTrackRange;
    public bool staminaCompletelyDepleted;
    public int enemyID;
    public float currentHealth, totalHealth, currentStamina, totalStamina, staminaRegenRate;
    public float staminaRegenInterval;
    float lastStaminaUsedTime;
    public bool poiseBroken;
    public float poiseBreakHealthLimit, poiseBreakHealthResetInterval;
    public float healthLeftTillPoiseBreak, lastPoiseBreakHealthResetTime;
    #endregion

    // Start is called before the first frame update
    void OnEnable()
    {
        fadePanelNow = false;
        playerInCombatMode = false;
        isPanelActive = true;
        poiseBroken = false;
        lastHealthStaminaUsedTime = lastPoiseBreakHealthResetTime = Time.time;

        if (isPlayer || isBoss)
        {
            panelUIRenderer = GameObject.Find("PlayerStatsPanel").GetComponent<CanvasGroup>();
            healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Image>();
            staminaBar = GameObject.Find("PlayerStaminaBar").GetComponent<Image>();
        }

        //Load from character script
        if (isPlayer)
        {
            totalHealth = PlayerManager.Instance.playerHealth;
            totalStamina = PlayerManager.Instance.playerStamina;
            staminaRegenRate = PlayerManager.Instance.staminaRegenRate;
            poiseBreakHealthLimit = PlayerManager.Instance.poiseBreakHealthLimit;
            poiseBreakHealthResetInterval = PlayerManager.Instance.poiseBreakHealthResetInterval;
        }
        else
        {
            totalHealth = EnemyManager.Instance.enemyData[enemyID].enemyHealth;
            totalStamina = EnemyManager.Instance.enemyData[enemyID].enemyStamina;
            staminaRegenRate = EnemyManager.Instance.enemyData[enemyID].staminaRegenRate;
            poiseBreakHealthLimit = EnemyManager.Instance.enemyData[enemyID].poiseBreakHealthLimit;
            poiseBreakHealthResetInterval = EnemyManager.Instance.enemyData[enemyID].poiseBreakHealthResetInterval;
        }

        currentHealth = totalHealth;
        currentStamina = totalStamina;
        staminaCompletelyDepleted = false;

        healthLeftTillPoiseBreak = poiseBreakHealthLimit;

        healthBar.fillAmount = currentHealth / totalHealth;
        staminaBar.fillAmount = currentStamina / totalStamina;
    }

    // Update is called once per frame
    void Update()
    {
        #region Panel Fading In Out
        if (isPanelActive)
        {
            if (isPlayer || isBoss)
            {
                if (!playerInCombatMode && Time.time - lastHealthStaminaUsedTime > panelFadeOutInterval)
                {
                    fadePanelNow = true;
                    isPanelActive = false;
                }
            }
            else
            {
                if (!playerInsideTrackRange && Time.time - lastHealthStaminaUsedTime > panelFadeOutInterval)
                {
                    fadePanelNow = true;
                    isPanelActive = false;
                }
            }
        }
        else if (fadePanelNow)
        {
            if (panelUIRenderer.alpha - (panelFadeOutSpeed * Time.deltaTime) > 0.0f)
            {
                panelUIRenderer.alpha -= panelFadeOutSpeed * Time.deltaTime;
            }
            else
            {
                panelUIRenderer.alpha = 0.0f;
                fadePanelNow = false;
            }
        }
        #endregion

        #region Health and Stamina regeneration
        if (currentStamina < totalStamina && Time.time - lastStaminaUsedTime > staminaRegenInterval)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            regenerateStamina(staminaRegenRate * Time.deltaTime);

            if (currentStamina >= totalStamina) staminaCompletelyDepleted = false;
        }

        if (Time.time - lastPoiseBreakHealthResetTime > poiseBreakHealthResetInterval)
        {
            lastPoiseBreakHealthResetTime = Time.time;
            healthLeftTillPoiseBreak = poiseBreakHealthLimit;
        }
        #endregion
    }

    public float modifyHealth(float healthAmount)
    {
        showHealthStaminaPanel();
        Debug.Log("Before kochi: " + healthAmount + "bichi: " + currentHealth + "kechi: " + healthLeftTillPoiseBreak);
        currentHealth += healthAmount;
        healthLeftTillPoiseBreak += healthAmount;
        Debug.Log("After kochi: " + healthAmount + "bichi: " + currentHealth + "kechi: " + healthLeftTillPoiseBreak);

        if (healthLeftTillPoiseBreak < 0.03f)
        {
            healthLeftTillPoiseBreak = 0.0f;
            poiseBroken = true;
        }
        else if (healthLeftTillPoiseBreak > poiseBreakHealthLimit) healthLeftTillPoiseBreak = poiseBreakHealthLimit;

        if (currentHealth >= totalHealth) currentHealth = totalHealth;
        else if (currentHealth <= 0.0f) currentHealth = 0.0f;

        applyHealthUIBarEffect(currentHealth);
        lastHealthStaminaUsedTime = Time.time;
        lastPoiseBreakHealthResetTime = Time.time;
        return currentHealth;
    }

    public void recoverPoise()
    {
        poiseBroken = false;
        healthLeftTillPoiseBreak = poiseBreakHealthLimit;
        lastPoiseBreakHealthResetTime = Time.time;
    }

    public float modifyStamina(float staminaAmount)
    {
        showHealthStaminaPanel();
        Debug.Log("Before kochi: " + staminaAmount + "bichi: " + currentStamina);
        currentStamina += staminaAmount;
        Debug.Log("After kochi: " + staminaAmount + "bichi: " + currentHealth);
        if (currentStamina < 0.0f) staminaCompletelyDepleted = true;
        else if (currentStamina > totalStamina) currentStamina = totalStamina;

        applyStaminaUIBarEffect(currentStamina);
        lastHealthStaminaUsedTime = lastStaminaUsedTime = Time.time;
        return currentStamina;
    }

    public void regenerateStamina(float staminaAmount)
    {
        showHealthStaminaPanel();
        currentStamina += staminaAmount;
        currentStamina = Mathf.Max(0.0f, currentStamina);

        applyStaminaUIBarEffect(currentStamina);
    }

    public void showHealthStaminaPanel()
    {
        isPanelActive = true;
        fadePanelNow = false;
        panelUIRenderer.alpha = 1.0f;
    }

    void applyHealthUIBarEffect(float newHealth)
    {
        healthBar.fillAmount = newHealth / totalHealth;
    }

    void applyStaminaUIBarEffect(float newStamina)
    {
        staminaBar.fillAmount = newStamina / totalStamina;
    }
}
