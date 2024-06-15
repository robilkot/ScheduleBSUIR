﻿using ScheduleBSUIR.Models;

namespace ScheduleBSUIR.Services
{
    public class GroupsService(WebService webService)
    {
        private readonly WebService _webService = webService;

        // todo: caching?
        public async Task<IEnumerable<StudentGroupHeader>> GetGroupHeadersAsync(string groupNameFilter, CancellationToken cancellationToken)
        {
            return await _webService.GetGroupHeadersAsync(groupNameFilter, cancellationToken) 
                ?? Enumerable.Empty<StudentGroupHeader>();
        }
    }
}
