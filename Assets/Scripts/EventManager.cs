using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public List<GameEvent> events = new List<GameEvent>();

    private void Awake()
    {
        Instance = this;
        CreateEvents();
    }

    public void TriggerRandomEvent()
    {
        if (events == null || events.Count == 0)
        {
            Debug.LogWarning("EventManager: no events to trigger.");
            return;
        }

        var ev = events[Random.Range(0, events.Count)];

        if (UIEventWindow.Instance == null)
        {
            Debug.LogWarning("UIEventWindow.Instance is null. Cannot display event.");
            return;
        }

        try
        {
            UIEventWindow.Instance.ShowEvent(ev);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while showing event: {ex}");
        }
    }

    void CreateEvents()
    {
        // 1) Слухи о диверсантах
        events.Add(new GameEvent(
            "Слухи о диверсантах",
            "Жители сообщают о подозрительных людях во дворах.",
            new EventChoice("Провести обыски", approval: -1, food: -1),
            new EventChoice("Игнорировать", approval: +1, probabilityOfSecondary: 0.25f), // шанс саботажа
            new EventChoice("Поручить Союзу Добрососедства", approval: +1, water: -1)
        ));

        // 2) Очередь за водой
        events.Add(new GameEvent(
            "Очередь за водой",
            "У колодцев образуются длинные очереди. Люди начинают ссориться.",
            new EventChoice("Ввести нормирование", approval: -1),
            new EventChoice("Раздать воду бесплатно", approval: +1, water: -2),
            new EventChoice("Союз организует порядок", approval: +1, food: -1)
        ));

        // 3) Самовольный отряд
        events.Add(new GameEvent(
            "Самовольный отряд",
            "Группа жителей предлагает самостоятельно патрулировать квартал.",
            new EventChoice("Запретить", approval: -1),
            new EventChoice("Разрешить", action: () => {
                // шанс потерять людей
                if (Random.value < 0.25f)
                    GameManager.Instance.ModifyResource("workers", -1);
            }),
            new EventChoice("Координировать через Союз", approval: +1, materials: -1)
        ));

        // 4) Тайный склад
        events.Add(new GameEvent(
            "Тайный склад",
            "Обнаружен частный склад с припасами, скрываемый от остальных.",
            new EventChoice("Конфисковать", approval: -2, food: +2),
            new EventChoice("Закрыть глаза", approval: -1),
            new EventChoice("Передать под управление Союза", approval: +1, food: +1)
        ));

        // 5) Усталость жителей (идеально на 2–3 день)
        events.Add(new GameEvent(
            "Усталость жителей",
            "Люди вымотаны постоянной тревогой и нехваткой сна.",
            new EventChoice("Усилить дисциплину", approval: -1),
            new EventChoice("Дать передышку", approval: +1, food: -1),
            new EventChoice("Собрание Союза", approval: +2, water: -1)
        ));
    }
}
