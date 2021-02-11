using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Debug = UnityEngine.Debug;
using Unity.Burst.Intrinsics;
using static Unity.Burst.Intrinsics.X86.Sse;
using static Unity.Burst.Intrinsics.X86.Sse2;
using static Unity.Burst.Intrinsics.X86.Sse3;
using static Unity.Burst.Intrinsics.X86.Ssse3;
using static Unity.Burst.Intrinsics.X86.Sse4_1;
using static Unity.Burst.Intrinsics.X86.Sse4_2;
using static Unity.Burst.Intrinsics.X86.Avx;
using static Unity.Burst.Intrinsics.X86.Avx2;

public class MyJobSystem : SystemBase
{
    NativeArray<int4> MySourceArray;
    NativeArray<int4> MyDestArray;

    [BurstCompile]
    public struct MyJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int4> InArray;

        [WriteOnly]
        public NativeArray<int4> OutArray;

        public void Execute(int index)
        {
            var data = InArray.ReinterpretLoad<v128>(index);
            var shuffled = shuffle_epi32(data, 3 << 0 | 2 << 2 | 1 << 4 | 0 << 6);
            OutArray.ReinterpretStore<v128>(index, shuffled);
        }
    }

    protected override void OnCreate()
    {
        MySourceArray = new NativeArray<int4>(1, Allocator.Persistent);
        MySourceArray[0] = new int4(100, 101, 102, 103);
        MyDestArray = new NativeArray<int4>(1, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        MySourceArray.Dispose();
        MyDestArray.Dispose();
    }

    protected override void OnUpdate()
    {
        new MyJob
        {
            InArray = MySourceArray,
            OutArray = MyDestArray,
        }
        .Schedule(MySourceArray.Length, 1)
        .Complete();
    }
}