using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.RPC;

namespace NPCEngine.API
{
    /// <summary>
    /// Data class containing service metadata.
    ///</summary>
    [Serializable()]
    public class ServiceMetadata
    {

        /// <summary>
        /// ID of the service.
        /// </summary>
        public string id;

        /// <summary>
        /// Name of the service class.
        /// </summary>
        public string service;

        /// <summary>
        /// Name of the API class.
        /// </summary>
        public string api_name;

        /// <summary>
        /// Path to the service's folder.
        /// </summary>
        public string path;

        /// <summary>
        /// Short service class description
        /// </summary>
        public string service_short_description;

        /// <summary>
        /// Long service class description
        /// </summary>
        public string service_description;

        /// <summary>
        /// Model specific readme.
        /// </summary>
        public string readme;

    }

    /// <summary>
    /// Service status enum.
    /// </summary>
    public enum ServiceStatus
    {
        UNKNOWN,
        STARTING,
        RUNNING,
        STOPPED,
        AWAITING,
        TIMEOUT,
        ERROR
    }

    /// <summary>
    /// <c>Control</c> provides RPC interface to control service.
    ///</summary>
    public class Control : RPCBase

    {

        public override string ServiceId { get { return "control"; } }

        //Compare in a coroutine   
        public IEnumerator StartService(string service_id)
        {
            var result = this.Run<List<string>, List<string>>("start_service", new List<string> { service_id, });

            while (!result.ResultReady)
            {
                yield return null;
            }
        }

        public IEnumerator StopService(string service_id)
        {
            var result = this.Run<List<string>, List<string>>("stop_service", new List<string> { service_id, });

            while (!result.ResultReady)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Get service status.
        /// </summary>
        /// <param name="service_id"></param>
        /// <param name="outputCallback"></param>
        /// <returns></returns>
        public IEnumerator GetServiceStatus(string service_id, Action<ServiceStatus> outputCallback)
        {
            var result = this.Run<List<string>, string>("get_service_status", new List<string> { service_id, });

            while (!result.ResultReady)
            {
                yield return null;
            }
            switch (result.Result)
            {
                case "running":
                    outputCallback(ServiceStatus.RUNNING);
                    break;
                case "starting":
                    outputCallback(ServiceStatus.STARTING);
                    break;
                case "error":
                    outputCallback(ServiceStatus.ERROR);
                    break;
                case "timeout":
                    outputCallback(ServiceStatus.TIMEOUT);
                    break;
                case "stopped":
                    outputCallback(ServiceStatus.STOPPED);
                    break;
                case "awaiting":
                    outputCallback(ServiceStatus.AWAITING);
                    break;
                default:
                    outputCallback(ServiceStatus.ERROR);
                    break;
            }
        }

        /// <summary>
        /// Restart service.
        /// </summary>
        /// <param name="service_id">Resolvable service name (i.e. id, type or API name)</param>
        /// <returns></returns>
        public IEnumerator RestartService(string service_id)
        {
            var result = this.Run<List<string>, string>("restart_service", new List<string> { service_id, });

            while (!result.ResultReady)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Get metadata for one service.
        /// </summary>
        /// <param name="service_id">Resolvable service name (i.e. id, type or API name)</param>
        /// <param name="outputCallback">Callback action to consume results.</param>
        /// <returns><c>ServiceMetadata</c> for resolved service.</returns>
        public IEnumerator GetServiceMetadata(string service_id, Action<ServiceMetadata> outputCallback)
        {
            var result = this.Run<List<string>, ServiceMetadata>("get_service_metadata", new List<string> { service_id, });

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        /// <summary>
        /// Get metadata for all the services.
        /// </summary>
        /// <param name="outputCallback">Callback action to consume results.</param>
        /// <returns>A list of <c>ServiceMetadata</c> for each service.</returns>
        public IEnumerator GetServicesMetadata(Action<List<ServiceMetadata>> outputCallback)
        {
            var result = this.Run<List<string>, List<ServiceMetadata>>("get_services_metadata", new List<string>());

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        public ResultFuture<List<ServiceMetadata>> GetServicesMetadataFuture()
        {
            return this.Run<List<string>, List<ServiceMetadata>>("get_services_metadata", new List<string> { });
        }

        public ResultFuture<ServiceMetadata> GetServiceMetadataFuture(string service_id)
        {
            return this.Run<List<string>, ServiceMetadata>("get_service_metadata", new List<string> { service_id, });
        }

        public ResultFuture<ServiceStatus> GetServiceStatusFuture(string service_id)
        {
            return this.Run<List<string>, ServiceStatus>("get_service_status", new List<string> { service_id, });
        }
    }
}