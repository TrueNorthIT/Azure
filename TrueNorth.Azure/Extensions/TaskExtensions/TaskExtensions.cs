using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueNorth.Azure.Extensions.TaskExtensions
{
    public static class TaskExtensions
    {
        public static async void Forget(this Task task, params Type[] acceptableExceptions)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TODO: consider whether derived types are also acceptable.
                if (!acceptableExceptions.Contains(ex.GetType()))
                    throw;
            }
        }
    }
}
