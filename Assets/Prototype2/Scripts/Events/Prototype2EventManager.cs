using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baricovats.Prototype2
{
    public sealed class Prototype2EventManager : MonoBehaviour
    {
        public static Prototype2EventManager Instance { get; private set; }
        public Prototype2EventWindowBinder window;
        public Prototype2EventData activeEvent;
        private readonly List<Prototype2EventData> allEvents = new List<Prototype2EventData>();
        private readonly HashSet<string> usedMainEvents = new HashSet<string>();
        private readonly HashSet<string> usedExtraEvents = new HashSet<string>();
        private GameStateManager stateManager;
        private FactionManager factionManager;

        private void Awake()
        {
            Instance = this;
            stateManager = GetComponent<GameStateManager>();
            factionManager = GetComponent<FactionManager>();
            CreateEvents();
        }

        public void ResetCampaign()
        {
            usedMainEvents.Clear();
            usedExtraEvents.Clear();
            activeEvent = null;
            if (window != null)
                window.Hide();
        }

        public void ShowEvent(Prototype2EventData data)
        {
            activeEvent = data;
            if (window != null) window.Show(data);
        }

        public void Resolve(EventSwipeDirection direction)
        {
            if (activeEvent == null) return;
            if (!activeEvent.hasChoices)
                return;
            var resolvedEvent = activeEvent;
            var choice = direction == EventSwipeDirection.Left ? activeEvent.left
                : direction == EventSwipeDirection.Right ? activeEvent.right
                : activeEvent.down;
            ApplyChoice(resolvedEvent, choice);

            if (direction == EventSwipeDirection.Down)
                ScheduleDeferral(resolvedEvent, choice, 1);

            activeEvent = null;
            if (window != null) window.Hide();

            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(resolvedEvent);
        }

        public void ResolveFinalEvent()
        {
            if (activeEvent == null || activeEvent.hasChoices)
                return;
            var resolvedEvent = activeEvent;
            activeEvent = null;
            if (window != null)
                window.Hide();
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(resolvedEvent);
        }

        public void ShowEnding(EndingId ending)
        {
            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return;

            string title;
            string body;
            switch (ending)
            {
                case EndingId.Defeat:
                    title = "Поражение";
                    body = string.IsNullOrEmpty(state.endingReason)
                        ? "Критическое состояние города привело к поражению."
                        : state.endingReason;
                    break;
                case EndingId.DefenseCommittee:
                    title = "Концовка Комитета Обороны";
                    body = "Комитет Обороны получил контроль над городом.";
                    break;
                case EndingId.CivilianCouncil:
                    title = "Концовка Гражданского Совета";
                    body = "Гражданский Совет получил контроль над городом.";
                    break;
                case EndingId.NeighborUnion:
                    title = "Концовка Союза Добрососедства";
                    body = "Союз Добрососедства получил контроль над городом.";
                    break;
                case EndingId.LowReputationVictory:
                    title = "Город выжил";
                    body = "Управление потеряно: репутация всех фракций осталась низкой.";
                    break;
                default:
                    title = "Город выжил";
                    body = "Репутация всех фракций осталась в среднем диапазоне.";
                    break;
            }

            state.endingReason = body;
            stateManager.NotifyChanged();

            ShowEvent(new Prototype2EventData
            {
                id = "ending_" + ending,
                title = title,
                body = body,
                isMandatory = true,
                hasChoices = false
            });
        }

        public void ShowMandatoryEvent()
        {
            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return;
            var mandatory = allEvents.Find(e => e.isMandatory && state.day >= e.minimumDay && state.day <= e.maximumDay);
            if (mandatory != null)
                ShowEvent(mandatory);
            else if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(null);
        }

        public void ShowExtraEvent()
        {
            var extra = ChooseExtraEvent();
            if (extra != null)
                ShowEvent(extra);
            else if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(null);
        }

        public void ShowMainEvent()
        {
            var main = ChooseMainEvent();
            if (main != null)
                ShowEvent(main);
            else if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(null);
        }

        public bool TryShowFollowUp(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return false;
            var followUp = allEvents.Find(e => e.id == eventId);
            if (followUp == null)
                return false;
            ShowEvent(followUp);
            return true;
        }

        public bool TryShowUltimatum(string sourceEventId, int deferralCount)
        {
            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return false;

            int count = Mathf.Max(1, deferralCount);
            string sourceTitle = "подписанное дело";
            var sourceEvent = allEvents.Find(e => e.id == sourceEventId);
            if (sourceEvent != null)
                sourceTitle = sourceEvent.title;

            bool repeated = count >= 2;
            string title = repeated
                ? $"ПРИКАЗ ОБ ОКОНЧАТЕЛЬНОМ РЕШЕНИИ ПО ДЕЛУ «{sourceTitle.ToUpperInvariant()}»"
                : $"УЛЬТИМАТИВНАЯ РЕЗОЛЮЦИЯ ПО ДЕЛУ «{sourceTitle.ToUpperInvariant()}»";
            string body = repeated
                ? "Срок отсрочки истек повторно. Настоящим приказом Администрации предписано принять окончательное решение до конца текущего дня."
                : "Двухдневный срок отсрочки истек. Настоящей резолюцией Администрации установлен окончательный срок ответа до конца текущего дня.";

            var eventData = new Prototype2EventData
            {
                id = repeated ? "ultimatum_2" : "ultimatum_1",
                source = "Канцелярия города",
                title = title,
                body = body,
                isMandatory = false,
                isRepeatable = false,
                minimumDay = state.day,
                maximumDay = state.day,
                leftChoice = "ПРИНЯТЬ",
                rightChoice = "ОТКЛОНИТЬ",
                downChoice = "ОТЛОЖИТЬ",
                left = repeated
                    ? Choice(front: -8, civil: -3, defense: -3, union: -3)
                    : Choice(front: -3, civil: -1, defense: -1, union: -1),
                right = repeated
                    ? Choice(front: -18, civil: -8, defense: -8, union: -8)
                    : Choice(front: -8, civil: -4, defense: -4, union: -4),
                down = repeated
                    ? Choice(food: -12, fuel: -12, weapons: -12, household: -12, front: -25,
                        civil: -12, defense: -12, union: -12)
                    : Choice(food: -5, fuel: -5, weapons: -5, household: -5, front: -12,
                        civil: -6, defense: -6, union: -6)
            };

            if (count < 2)
                eventData.down = Choice(food: -5, fuel: -5, weapons: -5, household: -5, front: -12,
                    civil: -6, defense: -6, union: -6);
            else
                eventData.down = Choice(food: -12, fuel: -12, weapons: -12, household: -12, front: -25,
                    civil: -12, defense: -12, union: -12);

            ShowEvent(eventData);
            return true;
        }

        private void ScheduleDeferral(Prototype2EventData sourceEvent, Prototype2EventChoice choice, int deferralCount)
        {
            if (choice != null && choice.triggerEffectAfterDays > 0)
                return;

            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return;

            state.delayedEffects.Add(new DelayedEffect
            {
                id = Guid.NewGuid().ToString(),
                sourceEventId = sourceEvent.id,
                triggerDay = state.day + 2,
                followUpEventId = "__ultimatum__",
                isResolved = false,
                deferralCount = sourceEvent.id.StartsWith("ultimatum_", StringComparison.Ordinal)
                    ? deferralCount + 1
                    : deferralCount
            });
        }

        private Prototype2EventData ChooseMainEvent()
        {
            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return null;

            foreach (var ev in allEvents)
            {
                if (ev.isMandatory || ev.id.StartsWith("extra_", StringComparison.Ordinal))
                    continue;
                if (ev.id == "main_chain_inventory" || ev.id == "main_chain_supply")
                    continue;
                if (usedMainEvents.Contains(ev.id))
                    continue;
                if (state.day < ev.minimumDay || state.day > ev.maximumDay)
                    continue;
                if (MatchesConditions(ev))
                {
                    usedMainEvents.Add(ev.id);
                    return ev;
                }
            }

            foreach (var ev in allEvents)
            {
                if (ev.isMandatory || ev.id.StartsWith("extra_", StringComparison.Ordinal))
                    continue;
                if (ev.id == "main_chain_inventory" || ev.id == "main_chain_supply")
                    continue;
                if (usedMainEvents.Contains(ev.id))
                    continue;
                if (state.day < ev.minimumDay || state.day > ev.maximumDay)
                    continue;
                usedMainEvents.Add(ev.id);
                return ev;
            }
            return null;
        }

        private Prototype2EventData ChooseExtraEvent()
        {
            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return null;

            var available = allEvents.FindAll(e => e.id.StartsWith("extra_", StringComparison.Ordinal)
                && state.day >= e.minimumDay && state.day <= e.maximumDay);
            if (available.Count == 0)
                return null;

            var nonUsed = available.FindAll(e => e.isRepeatable || !usedExtraEvents.Contains(e.id));
            if (nonUsed.Count == 0)
                return null;

            var ev = nonUsed[UnityEngine.Random.Range(0, nonUsed.Count)];
            if (!ev.isRepeatable)
                usedExtraEvents.Add(ev.id);
            return ev;
        }

        private bool MatchesConditions(Prototype2EventData ev)
        {
            var state = stateManager.State;
            switch (ev.id)
            {
                case "main_chain_inventory": return state.HasFlag("chain_inventory_started");
                case "main_chain_supply": return state.HasFlag("chain_supply_started");
                case "main_food_shortage": return state.food < 30;
                case "main_water_stop": return !state.heatingAndWaterAvailable;
                case "main_weapons_shortage": return state.weapons < 25;
                case "main_worker_fatigue": return state.householdGoods < 30;
                case "main_front_collapse": return state.frontStrength < 40;
                case "main_union_low": return state.GetReputation(FactionId.NeighborUnion) < 30;
                case "main_council_low": return state.GetReputation(FactionId.CivilianCouncil) < 30;
                case "main_defense_low": return state.GetReputation(FactionId.DefenseCommittee) < 30;
                case "main_high_union": return state.GetReputation(FactionId.NeighborUnion) >= 80;
                case "main_high_defense": return state.GetReputation(FactionId.DefenseCommittee) >= 80;
            }
            return false;
        }

        private void ApplyChoice(Prototype2EventData sourceEvent, Prototype2EventChoice choice)
        {
            if (choice == null)
                return;

            var state = stateManager != null ? stateManager.State : null;
            if (state == null)
                return;

            state.food = Mathf.Clamp(state.food + AmplifyEffect(choice.food), 0, 100);
            state.fuel = Mathf.Clamp(state.fuel + AmplifyEffect(choice.fuel), 0, 100);
            state.weapons = Mathf.Clamp(state.weapons + AmplifyEffect(choice.weapons), 0, 100);
            state.householdGoods = Mathf.Clamp(state.householdGoods + AmplifyEffect(choice.householdGoods), 0, 100);
            state.frontStrength = Mathf.Clamp(state.frontStrength + AmplifyEffect(choice.frontStrength), 0, 100);

            if (factionManager != null)
            {
                int civilBase = AmplifyReputationEffect(choice.civilianReputation);
                int defenseBase = AmplifyReputationEffect(choice.defenseReputation);
                int unionBase = AmplifyReputationEffect(choice.unionReputation);
                int civilEffect = civilBase;
                int defenseEffect = defenseBase;
                int unionEffect = unionBase;

                if (civilBase > 0)
                {
                    defenseEffect -= 2;
                    unionEffect -= 2;
                }
                if (defenseBase > 0)
                {
                    civilEffect -= 2;
                    unionEffect -= 2;
                }
                if (unionBase > 0)
                {
                    civilEffect -= 2;
                    defenseEffect -= 2;
                }

                factionManager.ChangeReputation(FactionId.CivilianCouncil, civilEffect);
                factionManager.ChangeReputation(FactionId.DefenseCommittee, defenseEffect);
                factionManager.ChangeReputation(FactionId.NeighborUnion, unionEffect);
            }

            if (!string.IsNullOrEmpty(choice.flagToAdd))
                state.AddFlag(choice.flagToAdd);
            if (!string.IsNullOrEmpty(choice.flagToRemove))
                state.RemoveFlag(choice.flagToRemove);

            if (choice.triggerEffectAfterDays > 0)
            {
                state.delayedEffects.Add(new DelayedEffect
                {
                    id = Guid.NewGuid().ToString(),
                    sourceEventId = sourceEvent.id,
                    triggerDay = state.day + choice.triggerEffectAfterDays,
                    followUpEventId = choice.followUpEventId,
                    isResolved = false
                });
            }

            stateManager.ClampResources();
        }

        private static int AmplifyEffect(int value)
        {
            if (value == 0)
                return 0;
            return Mathf.RoundToInt(Mathf.Sign(value) * Mathf.Max(1f, Mathf.Abs(value) * 1.5f));
        }

        private static int AmplifyReputationEffect(int value)
        {
            if (value == 0)
                return 0;
            return Mathf.RoundToInt(Mathf.Sign(value) * Mathf.Max(2f, Mathf.Abs(value) * 1.5f));
        }

        private static Prototype2EventChoice Choice(int food = 0, int fuel = 0, int weapons = 0,
            int household = 0, int front = 0, int civil = 0, int defense = 0, int union = 0,
            string addFlag = null, string removeFlag = null, int delay = 0, string followUp = null)
        {
            return new Prototype2EventChoice
            {
                food = food,
                fuel = fuel,
                weapons = weapons,
                householdGoods = household,
                frontStrength = front,
                civilianReputation = civil,
                defenseReputation = defense,
                unionReputation = union,
                flagToAdd = addFlag,
                flagToRemove = removeFlag,
                triggerEffectAfterDays = delay,
                followUpEventId = followUp
            };
        }

        private void CreateEvents()
        {
            allEvents.Add(Event("mandatory_artillery_day_1", "После обстрела", "Предписывается определить порядок действий после первого обстрела.", true,
                Choice(front: -10, civil: -2),
                Choice(food: -5, front: -5, union: -2),
                Choice(front: -5, defense: 2), 1, 1));

            var finalEvent = Event("mandatory_final_day_9", "Итоги кампании", "Предлагается утвердить итоговый акт девятидневной кампании.", true,
                Choice(),
                Choice(),
                Choice(), 9, 9);
            finalEvent.hasChoices = false;
            allEvents.Add(finalEvent);

            allEvents.Add(Event("main_food_shortage", "Выдача провизии", "Склад требует утвердить порядок выдачи запасов населению.", false,
                Choice(food: 10, civil: 3, union: 2),
                Choice(food: -5, defense: 3),
                Choice(food: 5, union: 4), 2, 8));

            allEvents.Add(Event("main_water_stop", "Вода и тепло", "Котельная требует утвердить меры приостановки отопления.", false,
                Choice(fuel: -5, civil: 3, union: 2),
                Choice(defense: 2, union: -3),
                Choice(union: 4, fuel: -5), 2, 8));

            allEvents.Add(Event("main_weapons_shortage", "Оружие для фронта", "Комитет Обороны требует утвердить срочную поставку оружия.", false,
                Choice(weapons: -10, defense: 5, front: 10),
                Choice(weapons: 5, defense: -3, civil: 2),
                Choice(defense: 2, union: 2), 2, 8));

            allEvents.Add(Event("main_worker_fatigue", "Смены мастерских", "Рабочие требуют утвердить порядок дальнейших смен.", false,
                Choice(household: 10, civil: 4),
                Choice(household: -5, defense: 3),
                Choice(union: 4, household: 5), 2, 8));

            allEvents.Add(Event("main_front_collapse", "Снабжение фронта", "Штаб требует утвердить срочную поставку на фронт.", false,
                Choice(weapons: -10, front: 10, defense: 5),
                Choice(front: -10, defense: -4),
                Choice(front: 5, union: 2), 3, 8));

            allEvents.Add(Event("main_union_low", "Требования Союза", "Союз Добрососедства требует утвердить новые гарантии районам.", false,
                Choice(union: 8, food: -5),
                Choice(union: -5, defense: 3),
                Choice(union: 10, civil: -2), 2, 8));

            allEvents.Add(Event("main_council_low", "Отчет по запасам", "Гражданский Совет требует подписать отчет о распределении провизии.", false,
                Choice(civil: 8, food: -5),
                Choice(civil: -4, defense: 3),
                Choice(civil: 5, union: 2), 2, 8));

            allEvents.Add(Event("main_defense_low", "Чрезвычайные полномочия", "Комитет Обороны требует утвердить порядок действий при нехватке оружия.", false,
                Choice(defense: 8, weapons: -5),
                Choice(defense: -5, civil: 3),
                Choice(defense: 5, union: 2), 2, 8));

            allEvents.Add(Event("main_high_union", "Помощь районов", "Союз Добрососедства предлагает подписать акт о передаче запасов.", false,
                Choice(union: 3, food: 10),
                Choice(union: -2, weapons: 5),
                Choice(union: 5, household: 5), 2, 8));

            allEvents.Add(Event("main_high_defense", "Мастерские для фронта", "Комитет Обороны предлагает утвердить план усиления фронта.", false,
                Choice(defense: 3, weapons: 10, front: 5),
                Choice(defense: -2, household: 10),
                Choice(defense: 5, civil: -2), 2, 8));

            allEvents.Add(Event("main_sabotage", "Недостача на складе", "Контрольная служба зафиксировала пропажу оборудования.", false,
                Choice(civil: -3, union: 2, defense: 2),
                Choice(civil: 3, union: -2),
                Choice(union: 4, addFlag: "chain_inventory_started", delay: 2, followUp: "main_chain_inventory"), 2, 8));

            allEvents.Add(Event("main_chain_inventory", "Проверка склада", "Комиссия требует подписать итоговый акт расследования.", false,
                Choice(food: 5, civil: 2),
                Choice(defense: 3, food: -3),
                Choice(union: 4, addFlag: "chain_inventory_resolved"), 2, 9));


            allEvents.Add(Event("main_black_market", "Нелегальный обмен", "Администрация требует утвердить меры против черного рынка.", false,
                Choice(food: 5, civil: -3),
                Choice(defense: 4, union: -3),
                Choice(union: 5, food: 3), 2, 8));

            allEvents.Add(Event("main_workforce", "Рабочие для котельной", "Котельная требует подписать приказ о перераспределении рабочих.", false,
                Choice(front: -5, union: 3),
                Choice(front: 5, defense: 2),
                Choice(union: 4, household: 4), 2, 8));

            allEvents.Add(Event("main_signal", "Радиосообщение с фронта", "Штаб передал на подпись приказ о проверке сообщения с фронта.", false,
                Choice(defense: 4, front: 8),
                Choice(civil: 3, union: 2),
                Choice(union: 3, civil: 2), 2, 8));

            allEvents.Add(Event("main_shelling", "Уцелевшие запасы", "Комиссия предлагает утвердить порядок выдачи уцелевших запасов.", false,
                Choice(food: 5, civil: 3),
                Choice(weapons: 5, defense: 3),
                Choice(union: 4), 2, 8));

            allEvents.Add(Event("main_chain_front", "Разведывательная операция", "Разведка передала на подпись план рискованного снабжения.", false,
                Choice(weapons: -8, front: 12, defense: 4, addFlag: "chain_supply_started", delay: 2, followUp: "main_chain_supply"),
                Choice(front: -5, defense: -3),
                Choice(union: 3, food: 5), 2, 8));

            allEvents.Add(Event("main_chain_supply", "Колонна снабжения", "Комиссия требует утвердить результат операции снабжения.", false,
                Choice(weapons: 8, front: 5, defense: 3),
                Choice(food: 5, civil: 3),
                Choice(union: 4, addFlag: "chain_supply_resolved"), 2, 9));

            allEvents.Add(Event("main_mobilization", "Мобилизация мастерских", "Комитет Обороны требует подписать приказ о военном режиме.", false,
                Choice(weapons: 12, front: 8, defense: 5, household: -8),
                Choice(household: 8, civil: 3, defense: -3),
                Choice(union: 3, weapons: 4, household: -3), 3, 8));

            allEvents.Add(Event("main_hospital", "Перевод больницы", "Гражданский Совет требует утвердить акт о переводе больницы.", false,
                Choice(civil: 5, household: 10, fuel: -5),
                Choice(union: 3, civil: -3, household: -5),
                Choice(union: 5, civil: 2, fuel: -3), 3, 8));

            allEvents.Add(Event("extra_patrol", "Ночной патруль", "Администрация передала на подпись распоряжение об усилении патрулей.", false,
                Choice(defense: 3, front: 3),
                Choice(civil: 3),
                Choice(union: 3, household: 3), 1, 8));

            allEvents.Add(Event("extra_radio", "Местное радио", "Канцелярия требует подписать распоряжение для местного радио.", false,
                Choice(union: 3),
                Choice(defense: 3),
                Choice(civil: 3), 1, 8));

            allEvents.Add(Event("extra_market", "Обмен запасами", "Торговцы передали на подпись распоряжение об обмене запасами.", false,
                Choice(food: 5, fuel: -3),
                Choice(fuel: 5, food: -3),
                Choice(union: 4), 1, 8));

            allEvents.Add(Event("extra_lost_letter", "Переписка лидеров", "Комиссия передала на подпись акт о переписке лидеров районов.", false,
                Choice(union: 3),
                Choice(defense: 3),
                Choice(civil: 3), 1, 8));

            allEvents.Add(Event("extra_storm", "Меры при непогоде", "Диспетчерская служба передала распоряжение о снабжении и связи.", false,
                Choice(fuel: -3, civil: 2),
                Choice(food: -3, union: 2),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_water_queue", "Очередь за водой", "Котельная передала распоряжение об организации очереди.", false,
                Choice(union: 4),
                Choice(civil: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_roof", "Ремонт крыш", "Комиссия передала акт о повреждении жилых домов.", false,
                Choice(civil: 4, food: -3),
                Choice(union: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_workshop", "Ремонт инструмента", "Мастерская передала распоряжение о восстановлении инструмента.", false,
                Choice(household: 5),
                Choice(weapons: 3),
                Choice(union: 3), 1, 8));

            allEvents.Add(Event("extra_notice", "Объявление Совета", "Гражданский Совет передал распоряжение для районов.", false,
                Choice(civil: 4),
                Choice(union: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_night", "Ночная тревога", "Дежурная служба передала акт о тревоге в районе.", false,
                Choice(defense: 3),
                Choice(civil: 3),
                Choice(union: 3), 1, 8));
        }

        private static Prototype2EventData Event(string id, string title, string body, bool mandatory,
            Prototype2EventChoice left, Prototype2EventChoice right, Prototype2EventChoice down,
            int minimumDay, int maximumDay)
        {
            string source;
            if (mandatory)
            {
                source = "Военное управление города";
            }
            else if (id.StartsWith("extra_", StringComparison.Ordinal))
            {
                source = "Администрация города";
            }
            else if (id.StartsWith("main_chain_", StringComparison.Ordinal)
                || id == "main_sabotage")
            {
                source = "Контрольная служба";
            }
            else
            {
                source = GetResolutionSource(id);
            }

            return new Prototype2EventData
            {
                id = id,
                source = source,
                title = title,
                body = body,
                isMandatory = mandatory,
                isRepeatable = id.StartsWith("extra_", StringComparison.Ordinal),
                minimumDay = minimumDay,
                maximumDay = maximumDay,
                leftChoice = "ПРИНЯТЬ",
                rightChoice = "ОТКЛОНИТЬ",
                downChoice = "ОТЛОЖИТЬ",
                left = left,
                right = right,
                down = down
            };
        }

        private static string GetResolutionSource(string id)
        {
            if (id.Contains("union"))
                return "Союз Добрососедства";
            if (id.Contains("council"))
                return "Гражданский Совет";
            if (id.Contains("defense") || id.Contains("weapons") || id.Contains("front"))
                return "Комитет Обороны";
            if (id.Contains("water") || id.Contains("work"))
                return "Управление Котельной";
            if (id.Contains("food") || id.Contains("market") || id.Contains("shell"))
                return "Управление Складом";
            return "Администрация города";
        }
    }
}
