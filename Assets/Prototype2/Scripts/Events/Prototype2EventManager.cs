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
            var resolvedEvent = activeEvent;
            var choice = direction == EventSwipeDirection.Left ? activeEvent.left
                : direction == EventSwipeDirection.Right ? activeEvent.right
                : activeEvent.down;
            ApplyChoice(resolvedEvent, choice);
            activeEvent = null;
            if (window != null) window.Hide();

            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.OnEventResolved(resolvedEvent);
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

            state.food = Mathf.Clamp(state.food + choice.food, 0, 100);
            state.fuel = Mathf.Clamp(state.fuel + choice.fuel, 0, 100);
            state.weapons = Mathf.Clamp(state.weapons + choice.weapons, 0, 100);
            state.householdGoods = Mathf.Clamp(state.householdGoods + choice.householdGoods, 0, 100);
            state.frontStrength = Mathf.Clamp(state.frontStrength + choice.frontStrength, 0, 100);

            if (factionManager != null)
            {
                factionManager.ChangeReputation(FactionId.CivilianCouncil, choice.civilianReputation);
                factionManager.ChangeReputation(FactionId.DefenseCommittee, choice.defenseReputation);
                factionManager.ChangeReputation(FactionId.NeighborUnion, choice.unionReputation);
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
            allEvents.Add(Event("mandatory_artillery_day_1", "Обстрел города", "Тяжелая ночь. Первый обстрел заставил часть района бежать в укрытия.", true,
                Choice(front: -10, civil: -2),
                Choice(food: -5, front: -5, union: -2),
                Choice(front: -5, defense: 2), 1, 1));

            allEvents.Add(Event("mandatory_final_day_9", "К девятому рассвету", "Город держится, но решения этого дня определят исход осады.", true,
                Choice(defense: 5),
                Choice(civil: 5),
                Choice(union: 5), 9, 9));

            allEvents.Add(Event("main_food_shortage", "Нехватка провизии", "Склад сообщает о риске голода в жилых кварталах.", false,
                Choice(food: 10, civil: 3, union: 2),
                Choice(food: -5, defense: 3),
                Choice(food: 5, union: 4), 2, 8));

            allEvents.Add(Event("main_water_stop", "Котельная остановилась", "Без отопления жители требуют немедленного решения.", false,
                Choice(fuel: -5, civil: 3, union: 2),
                Choice(defense: 2, union: -3),
                Choice(union: 4, fuel: -5), 2, 8));

            allEvents.Add(Event("main_weapons_shortage", "Запрос оружия", "Комитет требует срочной поставки оружия на фронт.", false,
                Choice(weapons: -10, defense: 5, front: 10),
                Choice(weapons: 5, defense: -3, civil: 2),
                Choice(defense: 2, union: 2), 2, 8));

            allEvents.Add(Event("main_worker_fatigue", "Усталость рабочих", "Рабочие требуют передышку после нескольких смен.", false,
                Choice(household: 10, civil: 4),
                Choice(household: -5, defense: 3),
                Choice(union: 4, household: 5), 2, 8));

            allEvents.Add(Event("main_front_collapse", "Фронт под давлением", "Линия обороны просит срочных поставок.", false,
                Choice(weapons: -10, front: 10, defense: 5),
                Choice(front: -10, defense: -4),
                Choice(front: 5, union: 2), 3, 8));

            allEvents.Add(Event("main_union_low", "Союз ставит ультиматум", "Лидеры соседских районов требуют большего внимания.", false,
                Choice(union: 8, food: -5),
                Choice(union: -5, defense: 3),
                Choice(union: 10, civil: -2), 2, 8));

            allEvents.Add(Event("main_council_low", "Совет требует отчета", "Гражданский Совет оспаривает распределение провизии.", false,
                Choice(civil: 8, food: -5),
                Choice(civil: -4, defense: 3),
                Choice(civil: 5, union: 2), 2, 8));

            allEvents.Add(Event("main_defense_low", "Комитет ставит ультиматум", "Комитет Обороны угрожает перейти к самовольным действиям.", false,
                Choice(defense: 8, weapons: -5),
                Choice(defense: -5, civil: 3),
                Choice(defense: 5, union: 2), 2, 8));

            allEvents.Add(Event("main_high_union", "Союз предлагает помощь", "При высокой репутации Союз готов распределить местные запасы.", false,
                Choice(union: 3, food: 10),
                Choice(union: -2, weapons: 5),
                Choice(union: 5, household: 5), 2, 8));

            allEvents.Add(Event("main_high_defense", "Комитет предлагает план", "Оборонцы предлагают усилить фронт ценой бытовых услуг.", false,
                Choice(defense: 3, weapons: 10, front: 5),
                Choice(defense: -2, household: 10),
                Choice(defense: 5, civil: -2), 2, 8));

            allEvents.Add(Event("main_sabotage", "Подозрение на саботаж", "На складе обнаружена пропажа оборудования.", false,
                Choice(civil: -3, union: 2, defense: 2),
                Choice(civil: 3, union: -2),
                Choice(union: 4, addFlag: "chain_inventory_started", delay: 2, followUp: "main_chain_inventory"), 2, 8));

            allEvents.Add(Event("main_chain_inventory", "Проверка склада", "Расследование пропажи подходит к критической точке.", false,
                Choice(food: 5, civil: 2),
                Choice(defense: 3, food: -3),
                Choice(union: 4, addFlag: "chain_inventory_resolved"), 2, 9));


            allEvents.Add(Event("main_black_market", "Черный рынок", "В квартале появился нелегальный обмен провизии.", false,
                Choice(food: 5, civil: -3),
                Choice(defense: 4, union: -3),
                Choice(union: 5, food: 3), 2, 8));

            allEvents.Add(Event("main_workforce", "Нехватка рабочих", "Котельная просит вернуть рабочих с фронта.", false,
                Choice(front: -5, union: 3),
                Choice(front: 5, defense: 2),
                Choice(union: 4, household: 4), 2, 8));

            allEvents.Add(Event("main_signal", "Радиосигнал", "Получено сообщение о возможном прорыве осады.", false,
                Choice(defense: 4, front: 8),
                Choice(civil: 3, union: 2),
                Choice(union: 3, civil: 2), 2, 8));

            allEvents.Add(Event("main_shelling", "Последствия обстрела", "После обстрела требуется срочно распределить уцелевшие запасы.", false,
                Choice(food: 5, civil: 3),
                Choice(weapons: 5, defense: 3),
                Choice(union: 4), 2, 8));

            allEvents.Add(Event("main_chain_front", "Разведданные с фронта", "Разведка предлагает рискованную операцию снабжения.", false,
                Choice(weapons: -8, front: 12, defense: 4, addFlag: "chain_supply_started", delay: 2, followUp: "main_chain_supply"),
                Choice(front: -5, defense: -3),
                Choice(union: 3, food: 5), 2, 8));

            allEvents.Add(Event("main_chain_supply", "Колонна снабжения", "Результат операции снабжения стал известен.", false,
                Choice(weapons: 8, front: 5, defense: 3),
                Choice(food: 5, civil: 3),
                Choice(union: 4, addFlag: "chain_supply_resolved"), 2, 9));

            allEvents.Add(Event("extra_patrol", "Ночная патрульная смена", "Администрация получила предложение усилить патрули.", false,
                Choice(defense: 3, front: 3),
                Choice(civil: 3),
                Choice(union: 3, household: 3), 1, 8));

            allEvents.Add(Event("extra_radio", "Радиопередача", "Нужен срочный ответ через местное радио.", false,
                Choice(union: 3),
                Choice(defense: 3),
                Choice(civil: 3), 1, 8));

            allEvents.Add(Event("extra_market", "Рынок района", "Торговцы предлагают обменяться ресурсами.", false,
                Choice(food: 5, fuel: -3),
                Choice(fuel: 5, food: -3),
                Choice(union: 4), 1, 8));

            allEvents.Add(Event("extra_lost_letter", "Потерянное письмо", "Обнаружена переписка местных лидеров.", false,
                Choice(union: 3),
                Choice(defense: 3),
                Choice(civil: 3), 1, 8));

            allEvents.Add(Event("extra_storm", "Сильный ветер", "Погода усложнила снабжение и связь.", false,
                Choice(fuel: -3, civil: 2),
                Choice(food: -3, union: 2),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_water_queue", "Очередь за водой", "Жители спорят из-за очереди у колодца.", false,
                Choice(union: 4),
                Choice(civil: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_roof", "Поврежденная крыша", "После ветра несколько домов остались без крыши.", false,
                Choice(civil: 4, food: -3),
                Choice(union: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_workshop", "Местная мастерская", "Мастерская предлагает отремонтировать инструмент.", false,
                Choice(household: 5),
                Choice(weapons: 3),
                Choice(union: 3), 1, 8));

            allEvents.Add(Event("extra_notice", "Объявление Совета", "Гражданский Совет просит распространить объявление.", false,
                Choice(civil: 4),
                Choice(union: 3),
                Choice(defense: 2), 1, 8));

            allEvents.Add(Event("extra_night", "Ночная тревога", "В одном районе прозвучала тревожная сирена.", false,
                Choice(defense: 3),
                Choice(civil: 3),
                Choice(union: 3), 1, 8));
        }

        private static Prototype2EventData Event(string id, string title, string body, bool mandatory,
            Prototype2EventChoice left, Prototype2EventChoice right, Prototype2EventChoice down,
            int minimumDay, int maximumDay)
        {
            return new Prototype2EventData
            {
                id = id,
                title = title,
                body = body,
                isMandatory = mandatory,
                isRepeatable = id.StartsWith("extra_", StringComparison.Ordinal),
                minimumDay = minimumDay,
                maximumDay = maximumDay,
                left = left,
                right = right,
                down = down
            };
        }
    }
}
