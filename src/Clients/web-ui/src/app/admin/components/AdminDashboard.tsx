"use client";

import { useState } from "react";
import { scraperService } from "@/services/scraperService";
import { FaRobot, FaCheckCircle, FaExclamationTriangle } from "react-icons/fa";
import FollowsManagement from "./FollowsManagement";
import SystemStats from "./SystemStats";

interface Props {
  accessToken: string;
}

export default function AdminDashboard({ accessToken }: Props) {
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<"idle" | "success" | "error">("idle");
  const [message, setMessage] = useState("");

  const handleStartScraping = async () => {
    setLoading(true);
    setStatus("idle");
    setMessage("");

    try {
      await scraperService.startScraping(accessToken);
      setStatus("success");
      setMessage("Tarama işlemi başarıyla kuyruğa eklendi. Worker servisleri arka planda çalışmaya başladı.");
    } catch (error: any) {
      setStatus("error");
      setMessage(error.message || "Bir hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200">
          <div className="flex items-center gap-3 mb-4">
            <div className="p-3 bg-indigo-100 text-indigo-600 rounded-lg">
              <FaRobot size={24} />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-900">Web Kazıyıcı (Scraper)</h2>
              <p className="text-sm text-gray-500">Bilet sitelerinden veri çekmeyi tetikler.</p>
            </div>
          </div>

          <div className="mt-6">
            <button
              onClick={handleStartScraping}
              disabled={loading}
              className={`w-full py-3 px-4 rounded-lg font-medium text-white transition-all
                ${loading
                  ? "bg-gray-400 cursor-not-allowed"
                  : "bg-indigo-600 hover:bg-indigo-700 shadow-md hover:shadow-lg"
                }`}
            >
              {loading ? "İşlem Başlatılıyor..." : "Tüm Siteleri Tara"}
            </button>
          </div>

          {status === "success" && (
            <div className="mt-4 p-3 bg-green-50 text-green-700 rounded-lg flex items-start gap-2 text-sm">
              <FaCheckCircle className="mt-1 flex-shrink-0" />
              <span>{message}</span>
            </div>
          )}

          {status === "error" && (
            <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-lg flex items-start gap-2 text-sm">
              <FaExclamationTriangle className="mt-1 flex-shrink-0" />
              <span>{message}</span>
            </div>
          )}
        </div>

        <SystemStats />
      </div>

      <FollowsManagement accessToken={accessToken} />
    </div>
  );
}


