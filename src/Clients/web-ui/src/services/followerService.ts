const API_URL = "http://localhost/api/followers";

export interface Follow {
  id: number;
  userId: string;
  artistId: string;
  createdAt: string;
}

export const followerService = {
  getAllFollows: async (accessToken: string): Promise<Follow[]> => {
    const response = await fetch(`${API_URL}/admin/all`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`,
      },
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        throw new Error("Bu işlem için yetkiniz yok.");
      }
      throw new Error("Takipler yüklenemedi.");
    }

    return response.json();
  },

  deleteFollow: async (accessToken: string, followId: number): Promise<boolean> => {
    const response = await fetch(`${API_URL}/${followId}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${accessToken}`,
      },
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        throw new Error("Bu işlem için yetkiniz yok.");
      }
      throw new Error("Takip silinemedi.");
    }

    return true;
  },
};
