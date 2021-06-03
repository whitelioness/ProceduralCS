using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using NLog;

namespace ComputeCS
{
    public static class Tasks
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Task GetCreateOrUpdateTask(
            AuthTokens tokens,
            string url,
            string path,
            Dictionary<string, object> queryParams,
            Dictionary<string, object> createParams,
            bool create
        )
        {
            try
            {
                var task = new GenericViewSet<Task>(
                    tokens,
                    url,
                    path
                ).GetByQueryParams(queryParams);
                
                if (task != null && create)
                {
                    if (new List<string>{"failed", "finished", "stopped"}.IndexOf(task.Status) != -1)
                    {
                        Logger.Debug($"Setting status to pending for Task: {task.UID}");
                        createParams.Add("status", "pending");
                    }

                    Logger.Info($"Updating Task: {task.UID}");
                    task = new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).PartialUpdate(
                        task.UID,
                        createParams,
                        new Dictionary<string, object> {{"runAs", "last"}}
                    );
                }

                return task;
            }
            catch (ArgumentException err)
            {
                if (create)
                {
                    // Merge the query_params with create_params
                    if (createParams == null)
                    {
                        createParams = queryParams;
                    }
                    else
                    {
                        createParams = queryParams
                            .Union(createParams)
                            .ToDictionary(s => s.Key, s => s.Value);
                    }

                    Logger.Info($"Creating new task with create params: {createParams}");
                    // Create the object
                    return new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).Create(createParams);
                }

                Logger.Error($"Got error: {err.Message} while trying to create task");
                return new Task {ErrorMessages = new List<string> {err.Message}};
            }
        }
        
        public static Task CreateParent(
            AuthTokens tokens,
            string url,
            string path,
            string taskName,
            Dictionary<string, object> overrides,
            bool create
        )
        {
            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", taskName}
            };
            if (overrides.ContainsKey("parent"))
            {
                taskQueryParams.Add("parent", overrides["parent"]);
            }
            
            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, string>
                    {
                        {"case_dir", "foam"},
                        {
                            "task_type", "parent"
                        }
                    }
                }
            };

            if (overrides.ContainsKey("copy_from"))
            {
                taskCreateParams.Add("copy_from", overrides["copy_from"]);
            }

            var task = new GenericViewSet<Task>(
                tokens,
                url,
                path
            ).GetOrCreate(
                taskQueryParams,
                taskCreateParams,
                create
            );
            
            if (task.ErrorMessages != null)
            {
                throw new Exception(task.ErrorMessages.First());
            }

            return task;
        }
    }
}