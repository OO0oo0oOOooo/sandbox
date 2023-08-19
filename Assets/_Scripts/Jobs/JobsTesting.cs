using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class JobsTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {   
        NativeList<JobHandle> jobHandlesList = new NativeList<JobHandle>(Allocator.Temp);
        for (int i = 0; i < 10; i++)
        {
            JobHandle jobHandle = ReallyToughTaskJob();
            jobHandlesList.Add(jobHandle);
        }
        JobHandle.CompleteAll(jobHandlesList.AsArray());
        jobHandlesList.Dispose();
    }

    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob job = new ReallyToughJob();
        return job.Schedule();
    }
}

[BurstCompile]
public struct ReallyToughJob : IJob
{
    public void Execute()
    {
        float value = 0;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
