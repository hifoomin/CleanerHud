using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.PostProcessing;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.XR;

namespace CleanerHud
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "AAAHIFU";
        public const string PluginName = "CleanerHud";
        public const string PluginVersion = "1.0.1";

        public static Color survivorColor;
        public static Sprite white;
        public static Material fontMaterial;
        public static ManualLogSource logger;
        public static List<BodyIndex> bodyIndexBlacklist = new();

        public bool runHooks = true;

        public void Awake()
        {
            logger = base.Logger;
            var texture = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/texWhite.png").WaitForCompletion();
            white = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0f, 0f));
            fontMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/Fonts/Bombardier/tmpBombDropshadow3D.mat").WaitForCompletion();
            var notificationPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/NotificationPanel2.prefab").WaitForCompletion();
            RemoveBackground(notificationPanel);

            var genericNotificationPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/GenericTransformationNotificationPanel.prefab").WaitForCompletion();
            RemoveBackground(genericNotificationPanel);

            var benthicPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/CloverVoid/CloverVoidTransformationNotificationPanel.prefab").WaitForCompletion();
            RemoveBackground(benthicPanel);

            var regenScrapPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapRegenTransformationNotificationPanel.prefab").WaitForCompletion();
            RemoveBackground(regenScrapPanel);

            var transPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/UI/VoidTransformationNotificationPanel.prefab").WaitForCompletion();
            RemoveBackground(transPanel);

            var defaultWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerDefaultWaveUI.prefab").WaitForCompletion();
            var defaultAnimator = defaultWaveUI.GetComponent<Animator>();
            defaultAnimator.enabled = false;
            var defaultTrans = defaultWaveUI.transform;
            var defaultFillBarRoot = defaultTrans.Find("FillBarRoot");
            var defaultFillBarRootRect = defaultFillBarRoot.GetComponent<RectTransform>();
            var kys12 = defaultFillBarRoot.gameObject.AddComponent<FillBarKillYourself>();
            kys12.idealPosition = new Vector3(-120f, -21.15f, 0f);
            defaultFillBarRootRect.localScale = new Vector3(0.725f, 1f, 1f);
            var defaultFillBarBackdrop = defaultFillBarRoot.Find("Fillbar Backdrop");
            var defaultFillBarBackdropImage = defaultFillBarBackdrop.GetComponent<Image>();
            defaultFillBarBackdropImage.enabled = false;
            var defaultFillBarBackdropInner = defaultFillBarRoot.Find("FillBar Backdrop Inner");
            var defaultFillBarBackdropInnerImage = defaultFillBarBackdropInner.GetComponent<Image>();
            defaultFillBarBackdropInnerImage.enabled = false;
            var defaultRemainingEnemies = defaultTrans.Find("RemainingEnemiesRoot");
            var defaultRemainingEnemiesTitle = defaultRemainingEnemies.Find("RemainingEnemiesTitle");
            var defaultRemainingEnemiesTitleMesh = defaultRemainingEnemiesTitle.GetComponent<TextMeshProUGUI>();
            defaultRemainingEnemiesTitleMesh.color = Color.white;
            var defaultRemainingEnemiesCounter = defaultRemainingEnemies.Find("RemainingEnemiesCounter");
            var defaultRemainingEnemiesCounterMesh = defaultRemainingEnemiesCounter.GetComponent<TextMeshProUGUI>();
            defaultRemainingEnemiesCounterMesh.color = Color.white;

            var currentWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(currentWaveUI);

            var currentBossWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentBossWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(currentBossWaveUI);

            var currentScavWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentBossScavWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(currentScavWaveUI);

            var currentLunarWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentBossLunarWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(currentLunarWaveUI);

            var currentMithrixWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentBossBrotherUI.prefab").WaitForCompletion();
            RemoveBgOutline(currentMithrixWaveUI);

            var bomb = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactBombWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(bomb, true);

            var comm = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactCommandWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(comm, true);

            var eni = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactEnigmaWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(eni, true);

            var glass = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactGlassWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(glass, true);

            var diss = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactMixEnemyWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(diss, true);

            var kin = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactSingleMonsterTypeWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(kin, true);

            var frail = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactWeakAssKneesUI.prefab").WaitForCompletion();
            RemoveBgOutline(bomb, true);

            var soul = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerCurrentArtifactBombWaveUI.prefab").WaitForCompletion();
            RemoveBgOutline(soul, true);

            var nextWaveUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/InfiniteTowerNextWaveUI.prefab").WaitForCompletion();
            var nextOffset = nextWaveUI.transform.Find("Offset");
            var nextTime = nextOffset.Find("TimeUntilNextWaveRoot");
            var nextBackdrop = nextTime.GetChild(0);
            var nextBackdropImage = nextBackdrop.GetComponent<Image>();
            nextBackdropImage.enabled = false;

            var enemyInfoPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/EnemyInfoPanel.prefab").WaitForCompletion().transform;
            var enemyInfoPanelImage = enemyInfoPanel.GetComponent<Image>();
            enemyInfoPanelImage.enabled = false;

            var innerFrame = enemyInfoPanel.Find("InnerFrame");
            var innerFrameImage = innerFrame.GetComponent<Image>();
            innerFrameImage.enabled = false;

            var monsterBodiesContainer = innerFrame.Find("MonsterBodiesContainer");
            var monsterBodyIconContainer = monsterBodiesContainer.Find("MonsterBodyIconContainer");
            var monsterBodyIconContainerImage = monsterBodyIconContainer.GetComponent<Image>();
            monsterBodyIconContainerImage.enabled = false;

            var inventoryContainer = innerFrame.Find("InventoryContainer");
            var inventoryDisplay = inventoryContainer.Find("InventoryDisplay");
            var inventoryDisplayImage = inventoryDisplay.GetComponent<Image>();
            inventoryDisplayImage.enabled = false;

            var gameEnd = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/GameEndReportPanel.prefab").WaitForCompletion();
            var safeAreaJUICEDLMAO = gameEnd.transform.Find("SafeArea (JUICED)");
            var headerArea = safeAreaJUICEDLMAO.Find("HeaderArea");
            var deathFlavorText = headerArea.Find("DeathFlavorText");
            var deathFlavorTextMesh = deathFlavorText.GetComponent<HGTextMeshProUGUI>();
            deathFlavorTextMesh.fontStyle = FontStyles.Normal;
            deathFlavorTextMesh.fontSizeMax = 30f;
            var resultArea = headerArea.Find("ResultArea");
            var resultLabel = resultArea.Find("ResultLabel");
            var resultLabelMesh = resultLabel.GetComponent<HGTextMeshProUGUI>();
            resultLabelMesh.material = fontMaterial;
            var bodyArea = safeAreaJUICEDLMAO.Find("BodyArea");
            var statsAndChatArea = bodyArea.Find("StatsAndChatArea");
            var statsContainer = statsAndChatArea.Find("StatsContainer");
            var borderImage = statsContainer.Find("BorderImage").gameObject;
            borderImage.SetActive(false);
            var statsAndPlayerNav = statsContainer.Find("Stats And Player Nav");
            var statsHeader = statsAndPlayerNav.Find("Stats Header");
            var statsHeaderImage = statsHeader.GetComponent<Image>();
            statsHeaderImage.enabled = false;
            var statsBody = statsContainer.Find("Stats Body");
            var statsBodyImage = statsBody.GetComponent<Image>();
            statsBodyImage.enabled = false;
            var scrollView = statsBody.Find("ScrollView");
            var viewport = scrollView.Find("Viewport");
            var content = viewport.Find("Content");
            var selectedDifficultyStrip = content.Find("SelectedDifficultyStrip");
            var selectedDifficultyStripImage = selectedDifficultyStrip.GetComponent<Image>();
            selectedDifficultyStripImage.enabled = false;
            var enabledArtifactsStrip = content.Find("EnabledArtifactsStrip");
            var enabledArtifactsStripImage = enabledArtifactsStrip.GetComponent<Image>();
            enabledArtifactsStripImage.enabled = false;
            var scrollbarVertical = scrollView.Find("Scrollbar Vertical");
            var scrollbarVerticalImage = scrollbarVertical.GetComponent<Image>();
            scrollbarVerticalImage.enabled = false;
            var slidingArea = scrollbarVertical.Find("Sliding Area");
            var handle = slidingArea.Find("Handle");
            var handleImage = handle.GetComponent<Image>();
            handleImage.enabled = false;
            var chatArea = statsAndChatArea.Find("ChatArea");
            var chatAreaImage = chatArea.GetComponent<Image>();
            chatAreaImage.enabled = false;
            var chatAreaRect = chatArea.GetComponent<RectTransform>();
            chatAreaRect.localPosition = new Vector3(441.5f, -311.666666666f, 0f);
            chatAreaRect.localEulerAngles = new Vector3(0f, 357f, 0f);
            chatAreaRect.sizeDelta = new Vector2(832f, 200f);

            var statStripTemplate = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StatStripTemplate.prefab").WaitForCompletion();
            var statStripTemplateImage = statStripTemplate.GetComponent<Image>();
            statStripTemplateImage.enabled = false;

            var chatBox = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox.prefab").WaitForCompletion();
            var chatBoxImage = chatBox.GetComponent<Image>();
            chatBoxImage.enabled = false;
            var permanentBG = chatBox.transform.Find("PermanentBG");
            var permanentBGImage = permanentBG.GetComponent<Image>();
            permanentBGImage.sprite = white;
            permanentBGImage.color = new Color32(0, 0, 0, 212);
            var permanentBGRect = permanentBG.GetComponent<RectTransform>();
            permanentBGRect.localPosition = new Vector3(0f, 1f, 0f);
            permanentBGRect.sizeDelta = new Vector2(0f, 48f);
            var standardRect = chatBox.transform.Find("StandardRect");
            var chatBoxScrollView = standardRect.Find("Scroll View");
            var chatBoxBackground = chatBoxScrollView.Find("Background");
            var chatBoxBackgroundImage = chatBoxBackground.GetComponent<Image>();
            chatBoxBackgroundImage.enabled = false;
            var chatBoxBorderImage = chatBoxScrollView.Find("BorderImage");
            var chatBoxBorderImageImage = chatBoxBorderImage.GetComponent<Image>();
            chatBoxBorderImageImage.enabled = false;
            var chatBoxScrollbarVertical = chatBoxScrollView.Find("Scrollbar Vertical");
            var chatBoxScrollbarVerticalScrollbar = chatBoxScrollbarVertical.GetComponent<Scrollbar>();
            chatBoxScrollbarVerticalScrollbar.enabled = true;
            var rightArea = bodyArea.Find("RightArea");
            var infoArea = rightArea.Find("InfoArea");
            var infoAreaBorderImage = infoArea.Find("BorderImage");
            var infoAreaBorderImageImage = infoAreaBorderImage.GetComponent<Image>();
            infoAreaBorderImageImage.enabled = false;
            var infoHeader = infoArea.Find("Info Header");
            var infoHeaderImage = infoHeader.GetComponent<Image>();
            infoHeaderImage.enabled = false;
            var infoBody = infoArea.Find("Info Body");
            var itemArea = infoBody.Find("ItemArea");
            var itemHeader = itemArea.Find("Item Header");
            var itemHeaderImage = itemHeader.GetComponent<Image>();
            itemHeaderImage.enabled = false;
            var itemAreaScrollView = itemArea.Find("ScrollView");
            var itemAreaScrollViewImage = itemAreaScrollView.GetComponent<Image>();
            itemAreaScrollViewImage.enabled = false;
            var itemAreaScrollbarVertical = itemAreaScrollView.Find("Scrollbar Vertical");
            var itemAreaScrollbarVerticalImage = itemAreaScrollbarVertical.GetComponent<Image>();
            itemAreaScrollbarVerticalImage.enabled = false;
            var unlockArea = infoBody.Find("UnlockArea");
            var unlockedHeader = unlockArea.Find("Unlocked Header");
            var unlockedHeaderImage = unlockedHeader.GetComponent<Image>();
            unlockedHeaderImage.enabled = false;
            var unlockAreaScrollView = unlockArea.Find("ScrollView");
            var unlockAreaScrollViewImage = unlockAreaScrollView.GetComponent<Image>();
            unlockAreaScrollViewImage.enabled = false;
            var unlockAreaScrollbarVertical = unlockAreaScrollView.Find("Scrollbar Vertical");
            var unlockAreaScrollbarVerticalImage = unlockAreaScrollbarVertical.GetComponent<Image>();
            unlockAreaScrollbarVerticalImage.enabled = false;

            var hudCountdownPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HudCountdownPanel.prefab").WaitForCompletion();
            var hudCountdownPanelRect = hudCountdownPanel.GetComponent<RectTransform>();
            var kys11 = hudCountdownPanelRect.gameObject.AddComponent<UIKillYourself>();
            kys11.idealPosition = new Vector3(0f, -200f, 0f);
            var juice = hudCountdownPanel.transform.Find("Juice");
            var container = juice.Find("Container");
            var backdrop = container.Find("Backdrop");
            var backdropImage = backdrop.GetComponent<Image>();
            backdropImage.enabled = false;
            var border = container.Find("Border");
            var containerBrderImage = border.GetComponent<Image>();
            containerBrderImage.enabled = false;
            var countdownTitleLabel = container.Find("CountdownTitleLabel");
            var countdownTitleLabelMesh = countdownTitleLabel.GetComponent<HGTextMeshProUGUI>();
            countdownTitleLabelMesh.fontSharedMaterial = fontMaterial;
            countdownTitleLabelMesh.color = Color.red;
            var countdownLabel = container.Find("CountdownLabel");
            var countdownLabelMesh = countdownLabel.GetComponent<HGTextMeshProUGUI>();
            countdownLabelMesh.fontSharedMaterial = fontMaterial;
            countdownLabelMesh.color = Color.red;

            var runChatBox = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox, In Run.prefab").WaitForCompletion();
            var runPermanentBG = runChatBox.transform.Find("PermanentBG");
            var inputField = runPermanentBG.Find("Input Field");
            var inputFieldImage = inputField.GetComponent<Image>();
            inputFieldImage.sprite = white;
            inputFieldImage.color = new Color32(0, 0, 0, 100);
            var runStandardRect = runChatBox.transform.Find("StandardRect");
            var runScrollView = runStandardRect.Find("Scroll View");
            var runScrollbarVertical = runScrollView.Find("Scrollbar Vertical");
            var runScrollbarVerticalScrollbar = runScrollbarVertical.GetComponent<Scrollbar>();
            runScrollbarVerticalScrollbar.enabled = true;

            var viendCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorCrosshair.prefab").WaitForCompletion();
            var viendCrosshairRect = viendCrosshair.GetComponent<RectTransform>();
            viendCrosshairRect.localScale = Vector3.one * 0.8f;

            var viendCorruption = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorCorruptionUISimplified.prefab").WaitForCompletion();
            var canvasGroup = viendCorruption.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.66f;

            var animator = viendCorruption.GetComponent<Animator>();
            animator.speed = 0.25f;

            var viendCorruptionRect = viendCorruption.GetComponent<RectTransform>();
            viendCorruptionRect.localPosition = new Vector3(0.5f, 0.5f, 0f);
            viendCorruptionRect.localScale = Vector3.one * 1.1f;

            var viendCorruptionFillRoot = viendCorruption.transform.Find("FillRoot");
            var viendCorruptionFillRootRect = viendCorruptionFillRoot.GetComponent<RectTransform>();
            viendCorruptionFillRootRect.localPosition = Vector3.zero;

            var text = viendCorruptionFillRoot.Find("Text");
            var textRect = text.GetComponent<RectTransform>();
            var textImage = text.GetComponent<Image>();
            textImage.enabled = false;
            textRect.localPosition = new Vector3(0f, -60f, 0f);

            var scoreboardStrip = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ScoreboardStrip.prefab").WaitForCompletion().transform;
            var longBackground = scoreboardStrip.Find("LongBackground");
            var longBackgroundImage = longBackground.GetComponent<Image>();
            longBackgroundImage.sprite = white;
            // Destroy(longBackground.GetComponent<HorizontalLayoutGroup>());

            var classBackground = longBackground.Find("ClassBackground");
            var classBackgroundImage = classBackground.GetComponent<Image>();
            classBackgroundImage.enabled = false;
            // classBackground.GetComponent<LayoutElement>().ignoreLayout = true;
            var classBackgroundRect = classBackground.GetComponent<RectTransform>();

            var moneyText = longBackground.Find("MoneyText");
            // moneyText.GetComponent<LayoutElement>().ignoreLayout = true;
            var moneyTextMesh = moneyText.GetComponent<HGTextMeshProUGUI>();
            moneyTextMesh.color = Color.white;
            var moneyTextRect = moneyText.GetComponent<RectTransform>();
            moneyTextRect.pivot = new Vector2(0.5f, 0.5f);
            moneyTextRect.localPosition = new Vector3(-280f, 0f, 0f);

            var nameLabel = longBackground.Find("NameLabel");
            // nameLabel.GetComponent<LayoutElement>().ignoreLayout = true;
            var nameLabelRect = nameLabel.GetComponent<RectTransform>();
            nameLabelRect.pivot = new Vector2(0.5f, 0.5f);
            nameLabelRect.localPosition = new Vector3(-330f, 0f, 0f);

            var itemsBackground = longBackground.Find("ItemsBackground");
            // itemsBackground.GetComponent<LayoutElement>().ignoreLayout = true;
            var itemsBackgroundImage = itemsBackground.GetComponent<Image>();
            itemsBackgroundImage.enabled = false;
            var itemsBackgroundRect = itemsBackground.GetComponent<RectTransform>();
            itemsBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
            itemsBackgroundRect.localPosition = new Vector3(100f, 5f, 0f);

            var equipmentBackground = longBackground.Find("EquipmentBackground");
            // equipmentBackground.GetComponent<LayoutElement>().ignoreLayout = true;
            var equipmentBackgroundImage = equipmentBackground.GetComponent<Image>();
            equipmentBackgroundImage.enabled = false;
            var equipmentBackgroundRect = equipmentBackground.GetComponent<RectTransform>();
            equipmentBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
            equipmentBackgroundRect.localPosition = new Vector3(475f, 1f, 0f);
            equipmentBackgroundRect.localScale = Vector3.one * 1.1f;

            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.UI.AllyCardController.Awake += AllyCardController_Awake;
            On.RoR2.UI.AllyCardController.UpdateInfo += AllyCardController_UpdateInfo;
            On.RoR2.UI.ScoreboardController.Rebuild += ScoreboardController_Rebuild;
            On.RoR2.UI.BuffDisplay.UpdateLayout += BuffDisplay_UpdateLayout;
            On.RoR2.UI.InfiniteTowerWaveProgressBar.OnEnable += InfiniteTowerWaveProgressBar_OnEnable;
            On.RoR2.UI.HUDBossHealthBarController.LateUpdate += HUDBossHealthBarController_LateUpdate;

            // survivor color, alpha is 103 for scoreboard strip (clone) => longbackground color
        }

        private void HUDBossHealthBarController_LateUpdate(On.RoR2.UI.HUDBossHealthBarController.orig_LateUpdate orig, HUDBossHealthBarController self)
        {
            var bossExists = self.currentBossGroup && self.currentBossGroup.combatSquad.memberCount > 0;
            self.container.SetActive(bossExists);
            if (bossExists)
            {
                var totalObservedHealth = self.currentBossGroup.totalObservedHealth;
                var totalMaxObservedMaxHealth = self.currentBossGroup.totalMaxObservedMaxHealth;
                var percentHealth = (totalMaxObservedMaxHealth == 0f) ? 0f : Mathf.Clamp01(totalObservedHealth / totalMaxObservedMaxHealth);

                self.delayedTotalHealthFraction = Mathf.Clamp(Mathf.SmoothDamp(self.delayedTotalHealthFraction, percentHealth, ref self.healthFractionVelocity, 0.1f, float.PositiveInfinity, Time.deltaTime), percentHealth, 1f);
                self.fillRectImage.fillAmount = percentHealth;
                self.delayRectImage.fillAmount = self.delayedTotalHealthFraction;

                HUDBossHealthBarController.sharedStringBuilder.Clear().Append(totalObservedHealth.ToString("#", CultureInfo.InvariantCulture)).Append("/").Append(totalMaxObservedMaxHealth.ToString("#", CultureInfo.InvariantCulture));

                self.healthLabel.SetText(HUDBossHealthBarController.sharedStringBuilder);
                self.bossNameLabel.SetText(self.currentBossGroup.bestObservedName, true);
                self.bossSubtitleLabel.SetText(self.currentBossGroup.bestObservedSubtitle, true);
            }
        }

        public void RemoveBgOutline(GameObject prefab, bool isAugment = false)
        {
            var offset = prefab.transform.Find("Offset");
            if (offset)
            {
                var offsetImage = offset.GetComponent<Image>();
                if (offsetImage)
                {
                    offsetImage.enabled = false;
                }

                var outline = offset.Find("Outline");
                if (outline)
                {
                    outline.gameObject.SetActive(false);
                }

                if (isAugment)
                {
                    var verticalLayout = offset.Find("VerticalLayout");
                    var waveTitle = verticalLayout.Find("Wave Title");
                    var waveTitleMesh = waveTitle.GetComponent<TextMeshProUGUI>();
                    waveTitleMesh.color = new Color32(255, 213, 250, 255);
                }
            }
        }

        private void InfiniteTowerWaveProgressBar_OnEnable(On.RoR2.UI.InfiniteTowerWaveProgressBar.orig_OnEnable orig, InfiniteTowerWaveProgressBar self)
        {
            orig(self);
            if (runHooks)
            {
                if (self.barImage.GetComponent<FillBarKillYourselfColor>() == null)
                {
                    self.barImage.gameObject.AddComponent<FillBarKillYourselfColor>();
                    self.barImage.sprite = white;
                }
                var fillBarRoot = self.barImage.transform.parent;
                var animated = fillBarRoot.Find("Animated");
                var animatedImage = animated.GetComponent<Image>();
                animatedImage.color = new Color(survivorColor.r * 0.5f, survivorColor.g * 0.5f, survivorColor.b * 0.5f, 1f);
            }
        }

        private void BuffDisplay_UpdateLayout(On.RoR2.UI.BuffDisplay.orig_UpdateLayout orig, BuffDisplay self)
        {
            if (runHooks)
            {
                var buffCount = self.buffIcons.Count;
                if (self.name == "BuffDisplayRoot")
                    self.rectTranform.localPosition = new Vector3(-25f * buffCount, -45f, 0f);
            }

            orig(self);
        }

        private void ScoreboardController_Rebuild(On.RoR2.UI.ScoreboardController.orig_Rebuild orig, ScoreboardController self)
        {
            orig(self);
            var container = self.container;
            if (container)
            {
                // Debug.LogError("container exists");
                var stripContainer = container.GetChild(0);
                if (stripContainer)
                {
                    // Debug.LogError("strip container exists");
                    var longBackground = stripContainer.GetChild(0);
                    if (longBackground)
                    {
                        // Debug.LogError("long background exists");

                        var longBackgroundImage = longBackground.GetComponent<Image>();
                        if (longBackgroundImage)
                        {
                            // Debug.LogError("long background image exists");
                            longBackgroundImage.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.15f);
                        }
                    }
                }
            }
        }

        public void RemoveBackground(GameObject prefab)
        {
            var canvasGroup = prefab.transform.GetChild(0);
            if (canvasGroup)
            {
                var backdrop = canvasGroup.Find("Backdrop");
                if (backdrop)
                    backdrop.gameObject.SetActive(false);
            }
        }

        private void AllyCardController_UpdateInfo(On.RoR2.UI.AllyCardController.orig_UpdateInfo orig, AllyCardController self)
        {
            orig(self);
            var healthbar = self.healthBar.transform;
            var backgroundPanel = healthbar.Find("BackgroundPanel");
            if (backgroundPanel)
            {
                var backgroundPanelImage = backgroundPanel.GetComponent<Image>();
                if (backgroundPanelImage)
                {
                    backgroundPanelImage.enabled = false;
                }
                if (backgroundPanel.childCount >= 2)
                {
                    var child1 = backgroundPanel.GetChild(1);
                    if (child1)
                    {
                        child1.gameObject.SetActive(false);
                    }
                }
                if (backgroundPanel.childCount >= 3)
                {
                    var child2 = backgroundPanel.GetChild(2);
                    if (child2)
                    {
                        child2.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void AllyCardController_Awake(On.RoR2.UI.AllyCardController.orig_Awake orig, AllyCardController self)
        {
            orig(self);
            var image = self.GetComponent<Image>();
            if (image)
            {
                image.enabled = false;
            }
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            self.StartCoroutine(ChangeStuff(self));
        }

        private IEnumerator ChangeStuff(HUD self)
        {
            // Main.logger.LogError("starting coroutine of changing everything");

            var canvasGroup = self.GetComponent<CanvasGroup>() ? self.GetComponent<CanvasGroup>() : self.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            yield return new WaitForSeconds(0.2f);

            if (!Run.instance)
            {
                yield break;
            }

            survivorColor = Color.black;

            var targetBodyObject = self.targetBodyObject;

            if (targetBodyObject)
            {
                var cb = targetBodyObject.GetComponent<CharacterBody>();
                if (cb)
                {
                    var bodyIndex = cb.bodyIndex;
                    if (bodyIndexBlacklist.Contains(bodyIndex))
                    {
                        canvasGroup.alpha = 1f;
                        runHooks = false;
                        yield break;
                    }
                    var survColor = cb.bodyColor;
                    survivorColor = EqualColorIntensity(survColor, 0.85f, 0.85f);
                }
            }

            var combatHealthBarViewer = self.combatHealthBarViewer;
            combatHealthBarViewer.healthBarDuration = 10f;

            var mainUIArea = self.mainUIPanel.transform;

            var upperRightCluster = self.gameModeUiRoot.transform;
            var runInfoHudPanel = upperRightCluster.GetChild(0);

            var timerPanel = runInfoHudPanel.Find("TimerPanel");
            if (timerPanel)
            {
                var timerPanelImage = timerPanel.GetComponent<Image>();
                timerPanelImage.enabled = false;
                var wormGear = timerPanel.Find("Wormgear").gameObject;
                wormGear.SetActive(false);
                var timerText = timerPanel.Find("TimerText");
                var timerTextMesh = timerText.GetComponent<HGTextMeshProUGUI>();
                timerTextMesh.color = Color.white;
            }

            var wavePanel = runInfoHudPanel.Find("WavePanel");
            if (wavePanel)
            {
                var wavePanelImage = wavePanel.GetComponent<Image>();
                wavePanelImage.enabled = false;
                var waveText = wavePanel.Find("WaveText");
                var waveTextMesh = waveText.GetComponent<HGTextMeshProUGUI>();
                waveTextMesh.color = Color.white;
            }

            var setDifficultyPanel = runInfoHudPanel.Find("SetDifficultyPanel");
            var setDifficultyPanelImage = setDifficultyPanel.GetComponent<Image>();
            setDifficultyPanelImage.enabled = false;

            var difficultyIcon = setDifficultyPanel.Find("DifficultyIcon");
            var difficultyIconRect = difficultyIcon.GetComponent<RectTransform>();
            difficultyIconRect.localPosition = new Vector3(20f, 0f, -0.5f);

            var difficultyBar = runInfoHudPanel.Find("DifficultyBar");
            var difficultyBarImage = difficultyBar.GetComponent<Image>();
            difficultyBarImage.enabled = false;

            /*
            var basedWormgear = difficultyBar.Find("Wormgear");
            var basedWormgearImage = basedWormgear.GetComponent<RawImage>();
            basedWormgearImage.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.10588235294f);
            */
            var scrollView = difficultyBar.Find("Scroll View");
            var scrollViewImage = scrollView.GetComponent<Image>();
            scrollViewImage.enabled = false;

            var viewport = scrollView.Find("Viewport");
            var content = viewport.Find("Content");

            for (int i = 0; i < content.childCount; i++)
            {
                var segment = content.GetChild(i);
                var segmentImage = segment.GetComponent<Image>();
                segmentImage.preserveAspect = true;
                segmentImage.color = new Color(survivorColor.r * (1f - (0.11111111111f * i)), survivorColor.g * (1f - (0.11111111111f * i)), survivorColor.b * (1f - (0.11111111111f * i)), 1f);
                var segmentRect = segment.GetComponent<RectTransform>();
                segmentRect.localScale = new Vector3(1f, 1.1f, 1f);
            }

            var segment1 = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 1f);
            var segment2 = new Color(survivorColor.r * (8f / 9f), survivorColor.g * (8f / 9f), survivorColor.b * (8f / 9f), 1f);
            var segment3 = new Color(survivorColor.r * (7f / 9f), survivorColor.g * (7f / 9f), survivorColor.b * (7f / 9f), 1f);
            var segment4 = new Color(survivorColor.r * (6f / 9f), survivorColor.g * (6f / 9f), survivorColor.b * (6f / 9f), 1f);
            var segment5 = new Color(survivorColor.r * (5f / 9f), survivorColor.g * (5f / 9f), survivorColor.b * (5f / 9f), 1f);
            var segment6 = new Color(survivorColor.r * (4f / 9f), survivorColor.g * (4f / 9f), survivorColor.b * (4f / 9f), 1f);
            var segment7 = new Color(survivorColor.r * (3f / 9f), survivorColor.g * (3f / 9f), survivorColor.b * (3f / 9f), 1f);
            var segment8 = new Color(survivorColor.r * (2f / 9f), survivorColor.g * (2f / 9f), survivorColor.b * (2f / 9f), 1f);
            var segment9 = new Color(survivorColor.r * (1f / 9f), survivorColor.g * (1f / 9f), survivorColor.b * (1f / 9f), 1f);

            var difficultyBarController = difficultyBar.GetComponent<DifficultyBarController>();
            difficultyBarController.segmentDefs[0] = new DifficultyBarController.SegmentDef() { color = segment1, debugString = "Easy", token = "DIFFICULTY_BAR_1" };
            difficultyBarController.segmentDefs[1] = new DifficultyBarController.SegmentDef() { color = segment2, debugString = "Medium", token = "DIFFICULTY_BAR_2" };
            difficultyBarController.segmentDefs[2] = new DifficultyBarController.SegmentDef() { color = segment3, debugString = "Hard", token = "DIFFICULTY_BAR_3" };
            difficultyBarController.segmentDefs[3] = new DifficultyBarController.SegmentDef() { color = segment4, debugString = "Very Hard", token = "DIFFICULTY_BAR_4" };
            difficultyBarController.segmentDefs[4] = new DifficultyBarController.SegmentDef() { color = segment5, debugString = "Insane", token = "DIFFICULTY_BAR_5" };
            difficultyBarController.segmentDefs[5] = new DifficultyBarController.SegmentDef() { color = segment6, debugString = "Impossible", token = "DIFFICULTY_BAR_6" };
            difficultyBarController.segmentDefs[6] = new DifficultyBarController.SegmentDef() { color = segment7, debugString = "I SEE YOU", token = "DIFFICULTY_BAR_7" };
            difficultyBarController.segmentDefs[7] = new DifficultyBarController.SegmentDef() { color = segment8, debugString = "I'M COMING FOR YOU", token = "DIFFICULTY_BAR_8" };
            difficultyBarController.segmentDefs[8] = new DifficultyBarController.SegmentDef() { color = segment9, debugString = "HAHAHAHA", token = "DIFFICULTY_BAR_9" };

            var image1 = difficultyBarController.images[0];
            var image1Kys = image1.gameObject.AddComponent<KillYor>();
            image1Kys.idealColor = segment1;

            var image2 = difficultyBarController.images[1];
            var image2Kys = image2.gameObject.AddComponent<KillYor>();
            image2Kys.idealColor = segment2;

            var image3 = difficultyBarController.images[2];
            var image3Kys = image3.gameObject.AddComponent<KillYor>();
            image3Kys.idealColor = segment3;

            var image4 = difficultyBarController.images[3];
            var image4Kys = image4.gameObject.AddComponent<KillYor>();
            image4Kys.idealColor = segment4;

            var image5 = difficultyBarController.images[4];
            var image5Kys = image5.gameObject.AddComponent<KillYor>();
            image5Kys.idealColor = segment5;

            var image6 = difficultyBarController.images[5];
            var image6Kys = image6.gameObject.AddComponent<KillYor>();
            image6Kys.idealColor = segment6;

            var image7 = difficultyBarController.images[6];
            var image7Kys = image7.gameObject.AddComponent<KillYor>();
            image7Kys.idealColor = segment7;

            var image8 = difficultyBarController.images[7];
            var image8Kys = image8.gameObject.AddComponent<KillYor>();
            image8Kys.idealColor = segment8;

            var image9 = difficultyBarController.images[8];
            var image9Kys = image9.gameObject.AddComponent<KillYor>();
            image9Kys.idealColor = segment9;

            var backdrop = scrollView.Find("Backdrop");
            /*
            var backdropImage = backdrop.GetComponent<Image>();
            backdropImage.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.02745098039f);
            */
            var markerBackdrop = difficultyBar.Find("Marker, Backdrop");
            var markerBackdropImage = markerBackdrop.GetComponent<Image>();
            markerBackdropImage.enabled = false;

            var outline = runInfoHudPanel.Find("OutlineImage").gameObject;
            outline.SetActive(false);

            var rightInfoBar = runInfoHudPanel.Find("RightInfoBar");
            var objectivePanel = rightInfoBar.Find("ObjectivePanel");
            var objectivePanelImage = objectivePanel.GetComponent<Image>();
            objectivePanelImage.enabled = false;

            var objectiveLabel2 = objectivePanel.Find("Label").gameObject;
            objectiveLabel2.SetActive(false);

            var springCanvas = mainUIArea.Find("SpringCanvas");
            var bottomLeftCluster = springCanvas.Find("BottomLeftCluster");

            var bottomCenterCluster = springCanvas.Find("BottomCenterCluster");

            var barRoots = bottomLeftCluster.Find("BarRoots");
            barRoots.parent = bottomCenterCluster;
            Destroy(barRoots.GetComponent<VerticalLayoutGroup>());
            var barRootsRect = barRoots.GetComponent<RectTransform>();
            barRootsRect.rotation = Quaternion.identity;
            barRootsRect.pivot = new Vector2(0.5f, 0.25f);
            barRootsRect.anchoredPosition = Vector2.zero;
            barRootsRect.sizeDelta = new Vector2(-400f, 100f);

            var levelDisplayCluster = barRoots.Find("LevelDisplayCluster");
            var levelDisplayClusterRect = levelDisplayCluster.GetComponent<RectTransform>();
            levelDisplayClusterRect.localPosition = new Vector3(-300f, 45f, 0f);
            var buffDisplayRoot = levelDisplayCluster.Find("BuffDisplayRoot");
            Destroy(buffDisplayRoot.GetComponent<HorizontalLayoutGroup>());
            buffDisplayRoot.parent = barRoots;

            var levelDisplayRoot = levelDisplayCluster.Find("LevelDisplayRoot");
            var levelDisplayRootRect = levelDisplayRoot.GetComponent<RectTransform>();
            var kys8 = levelDisplayRoot.gameObject.AddComponent<UIKillYourself>();
            kys8.idealPosition = new Vector3(308f, -27f, 0f);
            // kys8.rectTrans = levelDisplayRootRect;
            levelDisplayRootRect.pivot = new Vector2(0.5f, 0.5f);

            var expBarRoot = levelDisplayCluster.Find("ExpBarRoot");
            var expBarRootImage = expBarRoot.GetComponent<Image>();
            expBarRootImage.sprite = white;
            expBarRootImage.enabled = false;

            var expBarRootRect = expBarRoot.GetComponent<RectTransform>();
            expBarRootRect.localPosition = new Vector3(510.3f, -12.6f, 0f);
            expBarRoot.localScale = new Vector3(1.244f, 0.8f, 1f);

            try
            {
                var healthbarRoot = barRoots.Find("HealthbarRoot");
                var healthbarRootRect = healthbarRoot.GetComponent<RectTransform>();
                healthbarRootRect.localPosition = new Vector3(-210f, 45f, 0f);
                var healthbarRootImage = healthbarRoot.GetComponent<Image>();
                healthbarRootImage.sprite = white;

                var shrunkenRoot = healthbarRoot.Find("ShrunkenRoot");
                var child1 = shrunkenRoot.GetChild(0);
                child1.gameObject.SetActive(false);
                var child2 = shrunkenRoot.GetChild(1);
                child2.gameObject.SetActive(false);
                var shrunkenExpBarRoot = expBarRoot.Find("ShrunkenRoot");
                var shrunkenExpBarRootRect = shrunkenExpBarRoot.GetComponent<RectTransform>();
                shrunkenExpBarRootRect.localScale = new Vector3(1f, 1.6666666666f, 1f);
                shrunkenExpBarRootRect.localPosition = new Vector3(-338.6f, 15.25f);
                var fillPanel = shrunkenExpBarRoot.Find("FillPanel");
                var fillPanelImage = fillPanel.GetComponent<Image>();
                fillPanelImage.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.72156862745f);
            }
            catch { };

            var bottomRightCluster = springCanvas.Find("BottomRightCluster");
            var bottomRightGraphicRaycaster = bottomRightCluster.GetComponent<GraphicRaycaster>();

            var spectatorLabel = bottomCenterCluster.Find("SpectatorLabel");
            var spectatorLabelRect = spectatorLabel.GetComponent<RectTransform>();

            spectatorLabelRect.anchoredPosition = new Vector2(0f, 150f);

            var bottomCenterGraphicRaycaster = bottomCenterCluster.gameObject.AddComponent<GraphicRaycaster>();
            bottomCenterGraphicRaycaster.blockingObjects = bottomRightGraphicRaycaster.blockingObjects;
            bottomCenterGraphicRaycaster.ignoreReversedGraphics = bottomRightGraphicRaycaster.ignoreReversedGraphics;
            bottomCenterGraphicRaycaster.useGUILayout = bottomRightGraphicRaycaster.useGUILayout;

            var transparentColor = new Color(0, 0, 0, 0);

            var skillIcon1 = self.skillIcons[0];
            skillIcon1.cooldownRemapPanel = null;
            var skillIcon1Image = skillIcon1.GetComponent<Image>();
            skillIcon1Image.color = transparentColor;
            var cooldownPanel1 = skillIcon1.transform.Find("CooldownPanel").gameObject;
            cooldownPanel1.SetActive(false);
            var cooldownText1 = skillIcon1.transform.Find("CooldownText");
            var cooldownTextRect1 = cooldownText1.GetComponent<RectTransform>();

            cooldownTextRect1.localPosition = new Vector3(-18f, 19.5f, 0f);

            var cooldownTextMesh1 = cooldownText1.GetComponent<HGTextMeshProUGUI>();
            cooldownTextMesh1.color = Color.white;
            var isReadyPanel1 = skillIcon1.isReadyPanelObject;
            var isReadyPanelImage1 = isReadyPanel1.GetComponent<Image>();
            isReadyPanelImage1.color = survivorColor;
            var iconPanel1 = skillIcon1.iconImage;
            var iconPanelRect1 = iconPanel1.GetComponent<RectTransform>();
            iconPanelRect1.localScale = Vector3.one * 1.1f;
            var skillBackgroundPanel1 = skillIcon1.transform.Find("SkillBackgroundPanel").gameObject;
            skillBackgroundPanel1.SetActive(false);
            var skillStockRoot1 = skillIcon1.transform.GetChild(4);
            var skillStockRootText1 = skillStockRoot1.Find("StockText");
            var skillStockRootTextMesh1 = skillStockRootText1.GetComponent<HGTextMeshProUGUI>();
            skillStockRootTextMesh1.color = Color.white;

            var skillIcon2 = self.skillIcons[1];
            skillIcon2.cooldownRemapPanel = null;
            var skillIcon2Image = skillIcon2.GetComponent<Image>();
            var cooldownPanel2 = skillIcon2.transform.Find("CooldownPanel").gameObject;
            cooldownPanel2.SetActive(false);
            var cooldownText2 = skillIcon2.transform.Find("CooldownText");
            var cooldownTextRect2 = cooldownText2.GetComponent<RectTransform>();

            cooldownTextRect2.localPosition = new Vector3(-18f, 19.5f, 0f);

            var cooldownTextMesh2 = cooldownText2.GetComponent<HGTextMeshProUGUI>();
            cooldownTextMesh2.color = Color.white;
            skillIcon2Image.color = transparentColor;
            var isReadyPanel2 = skillIcon2.isReadyPanelObject;
            var isReadyPanelImage2 = isReadyPanel2.GetComponent<Image>();
            isReadyPanelImage2.color = survivorColor;
            var iconPanel2 = skillIcon2.iconImage;
            var iconPanelRect2 = iconPanel2.GetComponent<RectTransform>();
            iconPanelRect2.localScale = Vector3.one * 1.1f;
            var skillBackgroundPanel2 = skillIcon2.transform.Find("SkillBackgroundPanel").gameObject;
            skillBackgroundPanel2.SetActive(false);
            var skillStockRoot2 = skillIcon2.transform.GetChild(4);
            var skillStockRootText2 = skillStockRoot2.Find("StockText");
            var skillStockRootTextMesh2 = skillStockRootText2.GetComponent<HGTextMeshProUGUI>();
            skillStockRootTextMesh2.color = Color.white;

            var skillIcon3 = self.skillIcons[2];
            skillIcon3.cooldownRemapPanel = null;
            var skillIcon3Image = skillIcon3.GetComponent<Image>();
            skillIcon3Image.color = transparentColor;
            var cooldownPanel3 = skillIcon3.transform.Find("CooldownPanel").gameObject;
            cooldownPanel3.SetActive(false);
            var cooldownText3 = skillIcon3.transform.Find("CooldownText");
            var cooldownTextRect3 = cooldownText3.GetComponent<RectTransform>();

            cooldownTextRect3.localPosition = new Vector3(-18f, 19.5f, 0f);

            var cooldownTextMesh3 = cooldownText3.GetComponent<HGTextMeshProUGUI>();
            cooldownTextMesh3.color = Color.white;
            var isReadyPanel3 = skillIcon3.isReadyPanelObject;
            var isReadyPanelImage3 = isReadyPanel3.GetComponent<Image>();
            isReadyPanelImage3.color = survivorColor;
            var iconPanel3 = skillIcon3.iconImage;
            var iconPanelRect3 = iconPanel3.GetComponent<RectTransform>();
            iconPanelRect3.localScale = Vector3.one * 1.1f;
            var skillBackgroundPanel3 = skillIcon3.transform.Find("SkillBackgroundPanel").gameObject;
            skillBackgroundPanel3.SetActive(false);
            var skillStockRoot3 = skillIcon3.transform.GetChild(4);
            var skillStockRootText3 = skillStockRoot3.Find("StockText");
            var skillStockRootTextMesh3 = skillStockRootText3.GetComponent<HGTextMeshProUGUI>();
            skillStockRootTextMesh3.color = Color.white;

            var skillIcon4 = self.skillIcons[3];
            skillIcon4.cooldownRemapPanel = null;
            var skillIcon4Image = skillIcon4.GetComponent<Image>();
            skillIcon4Image.color = transparentColor;
            var cooldownPanel4 = skillIcon4.transform.Find("CooldownPanel").gameObject;
            cooldownPanel4.SetActive(false);
            var cooldownText4 = skillIcon4.transform.Find("CooldownText");
            var cooldownTextRect4 = cooldownText4.GetComponent<RectTransform>();

            cooldownTextRect4.localPosition = new Vector3(-18f, 19.5f, 0f);

            var cooldownTextMesh4 = cooldownText4.GetComponent<HGTextMeshProUGUI>();
            cooldownTextMesh4.color = Color.white;
            var isReadyPanel4 = skillIcon4.isReadyPanelObject;
            var isReadyPanelImage4 = isReadyPanel4.GetComponent<Image>();
            isReadyPanelImage4.color = survivorColor;
            var iconPanel4 = skillIcon4.iconImage;
            var iconPanelRect4 = iconPanel4.GetComponent<RectTransform>();
            iconPanelRect4.localScale = Vector4.one * 1.1f;
            var skillBackgroundPanel4 = skillIcon4.transform.Find("SkillBackgroundPanel").gameObject;
            skillBackgroundPanel4.SetActive(false);
            var skillStockRoot4 = skillIcon4.transform.GetChild(4);
            var skillStockRootText4 = skillStockRoot4.Find("StockText");
            var skillStockRootTextMesh4 = skillStockRootText4.GetComponent<HGTextMeshProUGUI>();
            skillStockRootTextMesh4.color = Color.white;

            var scaler = bottomRightCluster.Find("Scaler");
            scaler.parent = bottomCenterCluster;
            var scalerRect = scaler.GetComponent<RectTransform>();
            scalerRect.rotation = Quaternion.identity;

            scalerRect.pivot = new Vector2(0.5f, 0f);

            scalerRect.sizeDelta = new Vector2(-639f, -234f);

            scalerRect.anchoredPosition = new Vector2(62f, 98f);

            var sprintCluster = scaler.Find("SprintCluster").gameObject;
            sprintCluster.SetActive(false);
            var inventoryCluster = scaler.Find("InventoryCluster").gameObject;
            inventoryCluster.SetActive(false);
            /*
            var weaponSlot = scaler.Find("WeaponSlot");
            if (weaponSlot)
            {
                var displayRoot = weaponSlot.Find("DisplayRoot");
                var displayRootRect = displayRoot.GetComponent<RectTransform>();
                displayRootRect.localPosition = new Vector3(-5.5f, 16.85f, 0f);
                displayRootRect.localScale = new Vector3(0.77f, 0.775f, 0.77f);
                var equipmentTextBackgroundPanel = displayRoot.Find("EquipmentTextBackgroundPanel").gameObject;
                equipmentTextBackgroundPanel.SetActive(false);
                var isReadyPanel = displayRoot.Find("IsReadyPanel");
                var isReadyPanelImage = isReadyPanel.GetComponent<Image>();
                isReadyPanelImage.color = survivorColor;
                var bgPanel = displayRoot.Find("BGPanel");
                var bgPanelImage = bgPanel.GetComponent<Image>();
                bgPanelImage.color = survivorColor * 0.643f;
                var bgPanelRect = bgPanel.GetComponent<RectTransform>();
                bgPanelRect.localScale = Vector3.one * 1.1f;
                var iconPanel = displayRoot.Find("IconPanel");
                var iconPanelRect = iconPanel.GetComponent<RectTransform>();
                iconPanelRect.localScale = Vector3.one * 1.1f;
                scalerRect.localPosition = new Vector3(94f, -98f, 11.75943f);

                var weaponChargeBar = displayRoot.Find("WeaponChargeBar(Clone)");
                var weaponChargeBarRect = weaponChargeBar.GetComponent<RectTransform>();
                weaponChargeBarRect.localPosition = new Vector3(0f, 35f, 0f);
                weaponChargeBarRect.localEulerAngles = Vector3.zero;
                weaponChargeBarRect.localScale = new Vector3(0.5f, 0.09f, 1f);
                var redCharge = weaponChargeBar.Find("RedCharge");
                var redChargeImage = redCharge.GetComponent<Image>();
                redChargeImage.color = survivorColor * 0.5f;
                var charge = weaponChargeBar.Find("Charge");
                var chargeImage = charge.GetComponent<Image>();
                chargeImage.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.72156862745f);
            }
            */

            var survivorColorTopLeft = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 0.15f);

            var moneyRoot = self.moneyText.transform;
            var backgroundPanel = moneyRoot.Find("BackgroundPanel");
            var backgroundPanelImage = backgroundPanel.GetComponent<RawImage>();
            backgroundPanelImage.color = survivorColorTopLeft;
            var valueText = moneyRoot.Find("ValueText");
            var valueTextMesh = valueText.GetComponent<HGTextMeshProUGUI>();
            valueTextMesh.color = Color.white;
            var dollarSign = moneyRoot.Find("DollarSign");
            var dollarSignMesh = dollarSign.GetComponent<HGTextMeshProUGUI>();
            dollarSignMesh.color = Color.white;

            var upperLeftCluster = moneyRoot.parent;
            var upperLeftClusterImage = upperLeftCluster.GetComponent<Image>();
            upperLeftClusterImage.enabled = false;
            var upperLeftClusterVerticalLayoutGroup = upperLeftCluster.GetComponent<VerticalLayoutGroup>();
            upperLeftClusterVerticalLayoutGroup.spacing = 0;

            var lunarCoinRoot = upperLeftCluster.Find("LunarCoinRoot");
            var lunarCoinBackgroundPanel = lunarCoinRoot.Find("BackgroundPanel");
            var lunarCoinBackgroundPanelImage = lunarCoinBackgroundPanel.GetComponent<RawImage>();
            lunarCoinBackgroundPanelImage.color = survivorColorTopLeft;
            var lunarCoinValueText = lunarCoinRoot.Find("ValueText");
            var lunarCoinValueTextMesh = lunarCoinValueText.GetComponent<HGTextMeshProUGUI>();
            lunarCoinValueTextMesh.color = Color.white;
            var lunarCoinSign = lunarCoinRoot.Find("LunarCoinSign");
            var lunarCoinSignMesh = lunarCoinSign.GetComponent<HGTextMeshProUGUI>();
            lunarCoinSignMesh.color = Color.white;

            var itemInventoryDisplay = self.itemInventoryDisplay;
            var itemInventoryDisplayRoot = itemInventoryDisplay.transform.parent;
            var itemInventoryDisplayRootRect = itemInventoryDisplayRoot.GetComponent<RectTransform>();

            itemInventoryDisplayRootRect.localPosition = new Vector3(-26f, -90f, 0f);

            itemInventoryDisplayRootRect.anchoredPosition = new Vector2(550f, -90f);

            itemInventoryDisplayRootRect.pivot = new Vector2(0.5f, 0.5f);

            itemInventoryDisplayRootRect.sizeDelta = new Vector2(1000f, 140f);

            // var kys6 = itemInventoryDisplayRoot.gameObject.AddComponent<KillYors>();
            // kys6.inventory = itemInventoryDisplayRootRect;
            var itemInventoryDisplayImage = itemInventoryDisplay.GetComponent<Image>();

            itemInventoryDisplayImage.enabled = false;
            // goal is to have 18 icons per row instead of 20

            var topCenterCluster = springCanvas.Find("TopCenterCluster");

            Destroy(topCenterCluster.GetComponent<VerticalLayoutGroup>());

            var bossHealthBarRoot = topCenterCluster.Find("BossHealthBarRoot");
            var bossHealthBarRootRect = bossHealthBarRoot.GetComponent<RectTransform>();

            bossHealthBarRootRect.localPosition = new Vector3(0f, -160f, -3.81469706f);

            Destroy(bossHealthBarRoot.GetComponent<VerticalLayoutGroup>());

            var bossContainer = bossHealthBarRoot.Find("Container");

            Destroy(bossContainer.GetComponent<VerticalLayoutGroup>());

            var bossHealthBarContainer = bossContainer.Find("BossHealthBarContainer");
            var bossHealthBarContainerImage = bossHealthBarContainer.GetComponent<Image>();
            bossHealthBarContainerImage.enabled = false;

            var bossNameLabel = bossContainer.Find("BossNameLabel");
            var bossNameLabelRect = bossNameLabel.GetComponent<RectTransform>();

            bossNameLabelRect.localPosition = new Vector3(0f, 32.5f, 0f);

            var bossSubtitleLabel = bossContainer.Find("BossSubtitleLabel");
            var bossSubtitleLabelRect = bossSubtitleLabel.GetComponent<RectTransform>();
            var kys7 = bossSubtitleLabel.gameObject.AddComponent<UIKillYourself>();

            kys7.idealPosition = new Vector3(0f, -40f, 0f);

            // kys7.rectTrans = bossSubtitleLabelRect;
            var bossSubtitleLabelMesh = bossSubtitleLabel.GetComponent<HGTextMeshProUGUI>();

            bossSubtitleLabelMesh.color = Color.white;

            var bossBackgroundPanel = bossHealthBarContainer.Find("BackgroundPanel");
            var bossBackgroundPanelRect = bossBackgroundPanel.GetComponent<RectTransform>();

            bossBackgroundPanelRect.localPosition = new Vector3(0f, -42.5f, 0f);

            bossBackgroundPanelRect.localScale = new Vector3(1f, 1.5f, 1f);

            var bossBackgroundPanelImage = bossBackgroundPanel.GetComponent<Image>();
            bossBackgroundPanelImage.enabled = true;
            var delayFillPanel = bossBackgroundPanel.Find("DelayFillPanel");
            var delayFillPanelImage = delayFillPanel.GetComponent<Image>();

            delayFillPanelImage.color = new Color32(138, 0, 0, 255);

            var shieldPanel = bossBackgroundPanel.Find("ShieldPanel");
            var healthText = bossBackgroundPanel.Find("HealthText");
            var healthTextRect = healthText.GetComponent<RectTransform>();

            healthTextRect.localPosition = new Vector3(0f, 11.5f, -5.85840205f);

            healthTextRect.localEulerAngles = Vector3.zero;

            healthTextRect.localScale = new Vector3(1.25f, 0.8325f, 1.25f);

            var shieldPanelSiblingIndex = shieldPanel.GetSiblingIndex();
            var healthTextSiblingIndex = healthText.GetSiblingIndex();
            healthText.SetSiblingIndex(shieldPanelSiblingIndex);
            shieldPanel.SetSiblingIndex(healthTextSiblingIndex);

            var rightCluster = springCanvas.Find("RightCluster");
            var contextNotification = rightCluster.Find("ContextNotification");
            var contextDisplay = contextNotification.Find("ContextDisplay");
            var contextDisplayImage = contextDisplay.GetComponent<RawImage>();
            contextDisplayImage.enabled = false;

            var mainContainer = self.mainContainer.transform;
            var mapNameCluster = mainContainer.Find("MapNameCluster");
            var subtext = mapNameCluster.Find("Subtext");
            var subtextMesh = subtext.GetComponent<HGTextMeshProUGUI>();
            subtextMesh.color = Color.white;

            var notificationArea = mainContainer.Find("NotificationArea");
            var notificationAreaRect = notificationArea.GetComponent<RectTransform>();

            notificationAreaRect.localEulerAngles = new Vector3(0f, 6f, 0f);

            var kys4 = notificationArea.gameObject.AddComponent<KillYo>();
            kys4.notification = notificationAreaRect;

            var scoreboardPanel = springCanvas.Find("ScoreboardPanel");
            var container = scoreboardPanel.Find("Container");
            var stripContainer = container.Find("StripContainer");
            stripContainer.gameObject.AddComponent<FuckingKillYourself>();
            var stripContainerImage = stripContainer.GetComponent<Image>();
            stripContainerImage.enabled = false;
            var stripContainerRect = stripContainer.GetComponent<RectTransform>();

            stripContainerRect.sizeDelta = new Vector2(0f, 80f);

            var verticalLayoutGroup = stripContainer.GetComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childControlHeight = true;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childScaleHeight = true;
            verticalLayoutGroup.childScaleWidth = true;

            var buffDisplayRootRect = buffDisplayRoot.GetComponent<RectTransform>();

            buffDisplayRootRect.localPosition = new Vector3(-25f, -45f, 0f);
            /*
            var kys = buffDisplayRoot.gameObject.AddComponent<KillY>();
            kys.buffDisplayRoot = buffDisplayRootRect;
            */

            var equipment1 = self.equipmentIcons[0];
            var equipment1Rect = equipment1.GetComponent<RectTransform>();
            var kys2 = equipment1.gameObject.AddComponent<KillY>();
            kys2.equipment = equipment1Rect;
            var equipment1DisplayRoot = equipment1.displayRoot.transform;
            var equipment1DisplayRootRect = equipment1DisplayRoot.GetComponent<RectTransform>();

            equipment1DisplayRootRect.localPosition = new Vector3(-18f, 17.7f, 0f);

            equipment1DisplayRootRect.localScale = new Vector3(1f, 1.01f, 1f);
            var equipment1BGPanel = equipment1DisplayRoot.Find("BGPanel");
            var equipment1BGPanelRect = equipment1BGPanel.GetComponent<RectTransform>();
            equipment1BGPanelRect.localScale = Vector3.one * 1.1f;
            var equipment1IconPanel = equipment1DisplayRoot.Find("IconPanel");
            var equipment1IconPanelRect = equipment1IconPanel.GetComponent<RectTransform>();
            equipment1IconPanelRect.localScale = Vector3.one * 1.1f;
            var equipment1BGPanelImage = equipment1BGPanel.GetComponent<Image>();
            var equipmentIsReadyPanel1 = equipment1DisplayRoot.Find("IsReadyPanel");
            var equipmentIsReadyPanel1Image = equipmentIsReadyPanel1.GetComponent<Image>();
            equipmentIsReadyPanel1Image.color = survivorColor;
            equipment1BGPanelImage.color = survivorColor * 0.643f;
            var equipment1TextBackgroundPanel = equipment1DisplayRoot.Find("EquipmentTextBackgroundPanel").gameObject;
            equipment1TextBackgroundPanel.SetActive(false);
            var equipment1CooldownText = equipment1DisplayRoot.Find("CooldownText");
            var equipment1CooldownTextRect = equipment1CooldownText.GetComponent<RectTransform>();

            equipment1CooldownTextRect.localPosition = new Vector3(0f, 1f, 0f);

            var equipment2 = self.equipmentIcons[0];
            var equipment2Rect = equipment2.GetComponent<RectTransform>();
            var kys3 = equipment2.gameObject.AddComponent<KillY>();
            kys3.equipment = equipment2Rect;
            var equipment2DisplayRoot = equipment2.displayRoot.transform;
            var equipment2DisplayRootRect = equipment2DisplayRoot.GetComponent<RectTransform>();

            equipment2DisplayRootRect.localPosition = new Vector3(-23f, 22.7f, 0f);

            equipment2DisplayRootRect.localScale = new Vector3(1f, 1.01f, 1f);
            var equipment2BGPanel = equipment2DisplayRoot.Find("BGPanel");
            var equipment2BGPanelRect = equipment2BGPanel.GetComponent<RectTransform>();
            equipment2BGPanelRect.localScale = Vector3.one * 1.1f;
            var equipment2IconPanel = equipment2DisplayRoot.Find("IconPanel");
            var equipment2IconPanelRect = equipment2IconPanel.GetComponent<RectTransform>();
            equipment2IconPanelRect.localScale = Vector3.one * 1.1f;
            var equipment2BGPanelImage = equipment2BGPanel.GetComponent<Image>();
            var equipmentIsReadyPanel2 = equipment2DisplayRoot.Find("IsReadyPanel");
            var equipmentIsReadyPanel2Image = equipmentIsReadyPanel2.GetComponent<Image>();
            equipmentIsReadyPanel2Image.color = survivorColor;
            equipment2BGPanelImage.color = survivorColor * 0.643f;
            var equipment2TextBackgroundPanel = equipment2DisplayRoot.Find("EquipmentTextBackgroundPanel").gameObject;
            equipment2TextBackgroundPanel.SetActive(false);
            var equipment2CooldownText = equipment2DisplayRoot.Find("CooldownText");
            var equipment2CooldownTextRect = equipment2CooldownText.GetComponent<RectTransform>();

            equipment2CooldownTextRect.localPosition = new Vector3(0f, 1f, 0f);

            var artifactPanel = rightInfoBar.Find("ArtifactPanel");
            if (artifactPanel)
            {
                var artifactPanelImage = artifactPanel.GetComponent<Image>();
                artifactPanelImage.enabled = false;
            }

            /*
            if (Run.instance) difficultyBarController.SetSegmentScroll((Run.instance.ambientLevel - 1f) / difficultyBarController.levelsPerSegment);
            var mfChangePleaseAAAA = difficultyBarController.images[0];
            mfChangePleaseAAAA.color = new Color(survivorColor.r, survivorColor.g, survivorColor.b, 1f);
            */
            for (var t = 0f; t <= 2f; t += Time.deltaTime)
            {
                var percentCompleted = t / 2f;
                canvasGroup.alpha = Mathf.Lerp(0f, 0.7f, percentCompleted);
            }

            canvasGroup.alpha = 0.7f;
        }

        public Color EqualColorIntensity(Color color, float targetSaturation, float targetBrightness)
        {
            Color.RGBToHSV(color, out float h, out _, out _);

            var adjustedColor = Color.HSVToRGB(h, targetSaturation, targetBrightness);
            adjustedColor.a = color.a;

            return adjustedColor;
        }
    }

    public class KillY : MonoBehaviour
    {
        public RectTransform equipment;
        public Vector3 idealLocalPosition;
        public Vector3 idealLocalScale;
        public Vector3 idealSizeDelta;

        public void Start()
        {
            idealLocalPosition = new Vector3(138.9208f, -17.1797f, 0f);
            idealLocalScale = new Vector3(2.231415f, 2.231415f, 2.231415f);
            idealSizeDelta = new Vector2(36f, 36f);
        }

        public void Update()
        {
            if (!equipment)
            {
                return;
            }
            equipment.localPosition = idealLocalPosition;
            equipment.localScale = idealLocalScale;
            equipment.sizeDelta = idealSizeDelta;
        }
    }

    public class KillYo : MonoBehaviour
    {
        public RectTransform notification;
        public Vector3 idealLocalPosition;
        public Vector3 idealAnchor;

        public void Start()
        {
            idealLocalPosition = new Vector3(550f, -280f, 0f);
            idealAnchor = new Vector2(0.8f, 0.05f);
        }

        public void Update()
        {
            if (!notification)
            {
                return;
            }

            notification.localPosition = idealLocalPosition;
            notification.anchorMin = idealAnchor;
            notification.anchorMax = idealAnchor;
        }
    }

    public class KillYor : MonoBehaviour
    {
        public Color idealColor;
        public Image segment;

        public void Start()
        {
            segment = GetComponent<Image>();
        }

        public void Update()
        {
            segment.color = idealColor;
        }
    }

    /*
    public class KillYors : MonoBehaviour
    {
        public Vector3 idealLocalPosition;
        public Vector2 idealAnchoredPosition;
        public Vector2 idealPivot;
        public Vector2 idealSizeDelta;
        public RectTransform inventory;

        public void Start()
        {
            idealLocalPosition = new Vector3(-26f, -90f, 0f);
            idealAnchoredPosition = new Vector2(550f, -90f);
            idealPivot = new Vector2(0.5f, 0.5f);
            idealSizeDelta = new Vector2(1000f, 140f);
        }

        public void Update()
        {
            if (!inventory)
            {
                return;
            }

            inventory.localPosition = idealLocalPosition;
            inventory.anchoredPosition = idealAnchoredPosition;
            inventory.pivot = idealPivot;
            inventory.sizeDelta = idealSizeDelta;
        }
    }
    */

    public class UIKillYourself : MonoBehaviour
    {
        public Vector3 idealPosition;
        public RectTransform rectTrans;

        public void Start()
        {
            rectTrans = GetComponent<RectTransform>();
            rectTrans.localPosition = idealPosition;
        }

        /*
        public void Update()
        {
            if (!rectTrans)
            {
                return;
            }

            rectTrans.localPosition = idealPosition;
        }
        */
    }

    public class FillBarKillYourself : MonoBehaviour
    {
        public Vector3 idealPosition;
        public RectTransform rectTrans;

        public void Start()
        {
            rectTrans = GetComponent<RectTransform>();
        }

        public void Update()
        {
            rectTrans.localPosition = idealPosition;
        }
    }

    public class FillBarKillYourselfColor : MonoBehaviour
    {
        public Image fillBar;
        public Color fillBarColor;

        public void Start()
        {
            fillBar = GetComponent<Image>();
            fillBarColor = new Color(Main.survivorColor.r, Main.survivorColor.g, Main.survivorColor.b, 0.07f);
        }

        public void Update()
        {
            fillBar.color = fillBarColor;
        }
    }

    public class FuckingKillYourself : MonoBehaviour
    {
        public VerticalLayoutGroup verticalLayoutGroup;
        public Transform scoreboardStrip;
        public Transform longBackground;
        public RectTransform classBackground;
        public RectTransform nameLabel;
        public RectTransform moneyText;
        public RectTransform itemsBackground;
        public RectTransform equipmentBackground;
        public Vector2 idealClassBackgroundPivot;
        public Vector3 idealClassBackgroundPosition;
        public Vector3 idealClassBackgroundScale;

        public Vector2 idealNameLabelPivot;
        public Vector3 idealNameLabelPosition;

        public Vector2 idealMoneyTextPivot;
        public Vector3 idealMoneyTextPosition;

        public Vector2 idealItemsBackgroundPivot;
        public Vector3 idealItemsBackgroundPosition;

        public Vector2 idealEquipmentBackgroundPivot;
        public Vector3 idealEquipmentBackgroundPosition;
        public Vector3 idealEquipmentBackgroundScale;

        public void Start()
        {
            verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();

            idealClassBackgroundPivot = new Vector2(0.5f, 0.5f);
            idealClassBackgroundPosition = new Vector3(-476f, -0.2f, 0f);
            idealClassBackgroundScale = Vector3.one * 1.13f;

            idealNameLabelPivot = new Vector2(0.5f, 0.5f);
            idealNameLabelPosition = new Vector3(-320f, 0f, 0f);

            idealMoneyTextPivot = new Vector2(0.5f, 0.5f);
            idealMoneyTextPosition = new Vector3(-280f, 0f, 0f);

            idealItemsBackgroundPivot = new Vector2(0.5f, 0.5f);
            idealItemsBackgroundPosition = new Vector3(100f, 7f, 0f);

            idealEquipmentBackgroundPivot = new Vector2(0.5f, 0.5f);
            idealEquipmentBackgroundPosition = new Vector3(475f, 1f, 0f);
            idealEquipmentBackgroundScale = Vector3.one * 1.1f;
        }

        public void Update()
        {
            verticalLayoutGroup.enabled = false;

            scoreboardStrip = transform.GetChild(0);
            longBackground = scoreboardStrip.Find("LongBackground");
            classBackground = longBackground.Find("ClassBackground").GetComponent<RectTransform>();
            nameLabel = longBackground.Find("NameLabel").GetComponent<RectTransform>();
            moneyText = longBackground.Find("MoneyText").GetComponent<RectTransform>();
            itemsBackground = longBackground.Find("ItemsBackground").GetComponent<RectTransform>();
            equipmentBackground = longBackground.Find("EquipmentBackground").GetComponent<RectTransform>();

            classBackground.pivot = idealClassBackgroundPivot;
            // classBackground.localPosition = new Vector3(-473f, 0f, 0f);
            classBackground.localPosition = idealClassBackgroundPosition;
            classBackground.localScale = idealClassBackgroundScale;

            nameLabel.pivot = idealNameLabelPivot;
            nameLabel.localPosition = idealNameLabelPosition;

            moneyText.pivot = idealMoneyTextPivot;
            moneyText.localPosition = idealMoneyTextPosition;

            itemsBackground.pivot = idealItemsBackgroundPivot;
            itemsBackground.localPosition = idealItemsBackgroundPosition;

            equipmentBackground.pivot = idealEquipmentBackgroundPivot;
            equipmentBackground.localPosition = idealEquipmentBackgroundPosition;
            equipmentBackground.localScale = idealEquipmentBackgroundScale;

            verticalLayoutGroup.enabled = true;
        }
    }
}