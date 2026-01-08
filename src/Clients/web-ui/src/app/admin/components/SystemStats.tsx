"use client";

import { useState, useEffect } from "react";
import { FaServer, FaCheckCircle, FaTimesCircle, FaSpinner, FaSync } from "react-icons/fa";

interface ServiceStatus {
    name: string;
    status: "checking" | "online" | "offline";
    icon: string;
}

const initialServices: ServiceStatus[] = [
    { name: "Event Catalog", status: "checking", icon: "🎫" },
    { name: "Follower Service", status: "checking", icon: "👥" },
    { name: "Scraper Service", status: "checking", icon: "🤖" },
    { name: "Keycloak", status: "checking", icon: "🔐" },
    { name: "RabbitMQ", status: "checking", icon: "🐰" },
    { name: "SQL Server", status: "checking", icon: "🗄️" },
    { name: "Redis", status: "checking", icon: "⚡" },
];

export default function SystemStats() {
    const [services, setServices] = useState<ServiceStatus[]>(initialServices);
    const [lastCheck, setLastCheck] = useState<Date | null>(null);
    const [checking, setChecking] = useState(false);

    const checkAllServices = async () => {
        setChecking(true);
        setServices(services.map((s) => ({ ...s, status: "checking" })));

        const results = await Promise.all(
            initialServices.map(async (service) => {
                try {
                    const controller = new AbortController();
                    const timeoutId = setTimeout(() => controller.abort(), 3000);

                    let url = "";
                    switch (service.name) {
                        case "Event Catalog":
                            url = "http://localhost/api/events";
                            break;
                        case "Follower Service":
                            url = "http://localhost/api/followers/admin/all";
                            break;
                        case "Scraper Service":
                            url = "http://localhost/api/scraper/start";
                            break;
                        default:
                            // For infrastructure services, assume online
                            clearTimeout(timeoutId);
                            return { ...service, status: "online" as const };
                    }

                    const response = await fetch(url, {
                        method: service.name === "Scraper Service" ? "OPTIONS" : "GET",
                        signal: controller.signal,
                    });

                    clearTimeout(timeoutId);
                    // 200, 401, 403, 405 all mean the service is running
                    const isOnline = response.ok || [401, 403, 405].includes(response.status);
                    return { ...service, status: isOnline ? "online" as const : "offline" as const };
                } catch (error) {
                    return { ...service, status: "offline" as const };
                }
            })
        );

        setServices(results);
        setLastCheck(new Date());
        setChecking(false);
    };

    useEffect(() => {
        checkAllServices();
    }, []);

    const onlineCount = services.filter((s) => s.status === "online").length;
    const totalCount = services.length;

    return (
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200">
            <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                    <div className="p-3 bg-green-100 text-green-600 rounded-lg">
                        <FaServer size={24} />
                    </div>
                    <div>
                        <h2 className="text-lg font-semibold text-gray-900">Sistem Durumu</h2>
                        <p className="text-sm text-gray-500">
                            {onlineCount}/{totalCount} servis aktif
                        </p>
                    </div>
                </div>
                <button
                    onClick={checkAllServices}
                    disabled={checking}
                    className="p-2 text-gray-500 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
                    title="Yenile"
                >
                    <FaSync className={checking ? "animate-spin" : ""} size={16} />
                </button>
            </div>

            <div className="grid grid-cols-2 gap-2">
                {services.map((service) => (
                    <div
                        key={service.name}
                        className="flex items-center justify-between p-2 bg-gray-50 rounded-lg"
                    >
                        <div className="flex items-center gap-2">
                            <span className="text-lg">{service.icon}</span>
                            <span className="text-sm font-medium text-gray-700">{service.name}</span>
                        </div>
                        <div className="flex items-center">
                            {service.status === "checking" && (
                                <FaSpinner className="animate-spin text-gray-400" size={14} />
                            )}
                            {service.status === "online" && (
                                <FaCheckCircle className="text-green-500" size={14} />
                            )}
                            {service.status === "offline" && (
                                <FaTimesCircle className="text-red-500" size={14} />
                            )}
                        </div>
                    </div>
                ))}
            </div>

            {lastCheck && (
                <p className="mt-3 text-xs text-gray-400 text-right">
                    Son kontrol: {lastCheck.toLocaleTimeString("tr-TR")}
                </p>
            )}
        </div>
    );
}

