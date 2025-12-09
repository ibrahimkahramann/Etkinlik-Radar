const API_URL = "http://localhost/api/scraper";

export const scraperService = {
  startScraping: async (accessToken: string) => {
    const response = await fetch(`${API_URL}/start`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`,
      },
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        throw new Error("Bu işlem için yetkiniz yok (Admin rolü gerekli).");
      }
      throw new Error("Tarama başlatılamadı.");
    }

    return response.ok;
  },
};
