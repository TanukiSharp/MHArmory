#ifndef MAX_RESULTS
    #define MAX_RESULTS 128
#endif

#ifndef MAX_DESIRED_SKILLS
    #define MAX_DESIRED_SKILLS 0x80
#endif

#ifndef MAX_SET_SKILLS
    #define MAX_SET_SKILLS 0x40
#endif

#ifndef MAX_JEWEL_SIZE
    #define MAX_JEWEL_SIZE 3
#endif
#define SLOT_ARR_LEN MAX_JEWEL_SIZE + 1

#ifndef SLOTS_PER_EQUIPMENT
    #define SLOTS_PER_EQUIPMENT 3
#endif

#ifndef SKILLS_PER_EQUIPMENT
    #define SKILLS_PER_EQUIPMENT 3
#endif

#ifndef SET_SKILLS_PER_EQUIPMENT
    #define SET_SKILLS_PER_EQUIPMENT 3
#endif

#ifndef EQUIPMENT_TYPES
    #define EQUIPMENT_TYPES 6
#endif

typedef signed char int8_t;
typedef unsigned char uint8_t;
typedef signed short int16_t;
typedef unsigned short uint16_t;
typedef signed int int32_t;
typedef unsigned int uint32_t;
typedef signed long long int int64_t;
typedef unsigned long long int uint64_t;

typedef struct __attribute__((packed)) struct_skill_t
{
    int8_t id;
    int8_t level;
} skill_t;

typedef struct __attribute__((packed)) struct_set_skill_t
{
    uint8_t id;
    uint8_t requirement;
    skill_t skill;
} set_skill_t;

typedef struct __attribute__((packed)) struct_set_skill_progress_t
{
    uint8_t progress;
    uint8_t requirement;
    skill_t skill;
} set_skill_progress_t;

typedef struct __attribute__((packed)) struct_equipment_t
{
    uint16_t id;
    uint16_t original_id;
    skill_t skills[SKILLS_PER_EQUIPMENT];
    set_skill_t set_skills[SET_SKILLS_PER_EQUIPMENT];
    int8_t slots[SLOT_ARR_LEN];
} equipment_t;

typedef struct __attribute__((packed)) struct_deco_t
{
    uint8_t id;
    uint16_t original_id;
    int8_t slots;
    uint8_t available;
    skill_t skill;
} deco_t;

typedef struct __attribute__((packed)) struct_header_t
{
    int8_t weapon_slots[SLOT_ARR_LEN];
    uint8_t equipment_counts[EQUIPMENT_TYPES];
    uint8_t deco_count;
    int8_t desired_skill_count;
    int8_t set_skill_count;
} header_t;

typedef struct __attribute__((packed)) struct_deco_usage_t
{
    uint16_t original_id;
    int8_t count;
} deco_usage_t;

typedef struct __attribute__((packed)) struct_result_t
{
    uint16_t equipment_ids[6];
    uint8_t deco_count;
    deco_usage_t decos[(EQUIPMENT_TYPES + 1) * 3];
    int8_t spare_slots[SLOT_ARR_LEN];
} result_t;

bool is_match(const constant header_t* header, const equipment_t* equipments, const constant deco_t* decos, const constant skill_t* desired_skills, result_t* result)
{
    bool match = true;

    result->deco_count = 0;

    // Init skills
    int8_t skills_remaining[MAX_DESIRED_SKILLS];
    for (size_t i = 0; i < header->desired_skill_count; i++)
    {
        skills_remaining[i] = desired_skills[i].level;
    }
    
    // Init set part progress
    set_skill_progress_t set_skills[MAX_SET_SKILLS];
    for (size_t i = 0; i < header->set_skill_count; i++)
    {
        set_skills[i].progress = 0;
    }

    // Init slots
    int8_t total_slots[SLOT_ARR_LEN];
    for (size_t i = 0; i < SLOT_ARR_LEN; i++)
    {
        total_slots[i] = header->weapon_slots[i];
    }

    for (size_t i = 0; i < EQUIPMENT_TYPES; i++)
    {
        const equipment_t* equipment = &equipments[i];

        // Basic skills
        for (size_t j = 0; j < SKILLS_PER_EQUIPMENT; j++)
        {
            const skill_t equipment_skill = equipment->skills[j];
            skills_remaining[equipment_skill.id] -= equipment_skill.level;
        }

        // Accumulating set skills
        for (size_t j = 0; j < SET_SKILLS_PER_EQUIPMENT; j++)
        {
            const set_skill_t set_skill = equipment->set_skills[j];
            if (set_skill.id >= MAX_SET_SKILLS)
            {
                continue;
            }
            set_skill_progress_t* progress = &set_skills[set_skill.id];
            progress->requirement = set_skill.requirement;
            progress->skill = set_skill.skill;
            progress->progress++;
        }

        // Counting slots
        for (size_t j = 0; j < SLOT_ARR_LEN; j++)
        {
            total_slots[j] += equipment->slots[j];
        }

        result->equipment_ids[i] = equipment->original_id;
    }

    // Finalizing set skills
    for (size_t i = 0; i < header->set_skill_count; i++)
    {
        if (set_skills[i].progress >= set_skills[i].requirement)
        {
            skills_remaining[set_skills[i].skill.id] -= set_skills[i].skill.level;
        }
    }

    // Slotting decorations
    for (size_t i = 0; i < header->deco_count; i++)
    {
        const deco_t deco = decos[i];
        const int8_t missing = skills_remaining[deco.skill.id];
        if (missing > 0 && missing <= deco.available)
        {
            int8_t need_decos = missing < deco.available ? missing : deco.available;
            int8_t taken_decos = 0;
            for (size_t j = deco.slots; j < SLOT_ARR_LEN; j++)
            {
                int8_t slots = total_slots[j];
                int8_t taken_from_slot = need_decos < slots ? need_decos : slots;
                total_slots[j] -= taken_from_slot;
                taken_decos += taken_from_slot;
                need_decos -= taken_from_slot;
            }
            if (taken_decos > 0)
            {
                skills_remaining[deco.skill.id] -= taken_decos;
                deco_usage_t* usage = &result->decos[result->deco_count];
                usage->original_id = deco.original_id;
                usage->count = taken_decos;
                ++result->deco_count;
            }
        }
    }

    // Setting spare slots
    for (size_t i = 0; i < SLOT_ARR_LEN; i++)
    {
        result->spare_slots[i] = total_slots[i];
    }

    // Verifying match
    for (size_t i = 0; i < header->desired_skill_count; i++)
    {
        match &= skills_remaining[i] <= 0;
    }

    return match;
}

kernel void search(const constant header_t* header, const constant equipment_t* all_equipment, const constant deco_t* all_decos, const constant skill_t* desired_skills, global uint32_t* result_count, global write_only result_t* results)
{
    size_t combination = get_global_id(0);

    equipment_t equipments[EQUIPMENT_TYPES];
    size_t offset = 0;
    for (size_t i = 0; i < EQUIPMENT_TYPES; i++)
    {
        uint8_t equipment_count = header->equipment_counts[i];
        size_t index = combination % equipment_count + offset;
        equipments[i] = all_equipment[index];
        combination /= equipment_count;
        offset += equipment_count;
    }
    result_t result;
    bool match = is_match(header, equipments, all_decos, desired_skills, &result);
    if (match)
    {
        uint16_t result_idx = atomic_inc(result_count);
        if (result_idx > MAX_RESULTS - 1)
        {
            atomic_dec(result_count);
            return;
        }
        results[result_idx] = result;
    }
}
