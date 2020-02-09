# Bees data structure
- IComponentData
    - Position
    - Velocity
    - Smooth position
    - Smooth direction
    - Size (Possibly shared?)
    - Enemy Target (Added when appropriate)
    - Resource Target (Added when appropriate)
    - DeathTimer (Added when bee gets dying tag)
- SharedComponentData
    - Team
    - Color
    - Mesh
    - Material


- Beeparticle is Effects.

- Ideas:
    - Custom rendering. Do everything to avoid having to re-set GFX variables.
        - Can potentially gather stuff in jobs (in pointers), before sending it to the graphics system
    - Check how often the chunks update their versions, might be that we can cache allies and enemies.


# StayInFieldSystem:
Initial: "float3(-7.32777f, 24.83943f, 16.39395f)"
i = 0: "float3(3.663885f, 19.87154f, 13.11516f)"
i = 1: "float3(-1.831942f, 15.89723f, 10.49213f)"
i = 2: "float3(0.9159712f, 12.71779f, 8.393701f)"
