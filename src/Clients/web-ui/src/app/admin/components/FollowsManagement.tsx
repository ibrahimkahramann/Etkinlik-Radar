"use client";

import { useState, useEffect } from "react";
import { followerService, Follow } from "@/services/followerService";
import { FaUsers, FaTrash, FaSpinner } from "react-icons/fa";

interface Props {
    accessToken: string;
}

export default function FollowsManagement({ accessToken }: Props) {
    const [follows, setFollows] = useState<Follow[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [deletingId, setDeletingId] = useState<number | null>(null);

    const fetchFollows = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await followerService.getAllFollows(accessToken);
            setFollows(data);
        } catch (err: any) {
            setError(err.message || "Takipler yüklenemedi.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchFollows();
    }, [accessToken]);

    const handleDelete = async (followId: number) => {
        if (!confirm("Bu takibi silmek istediğinize emin misiniz?")) return;

        try {
            setDeletingId(followId);
            await followerService.deleteFollow(accessToken, followId);
            setFollows(follows.filter((f) => f.id !== followId));
        } catch (err: any) {
            alert(err.message || "Silme işlemi başarısız.");
        } finally {
            setDeletingId(null);
        }
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString("tr-TR");
    };

    const truncateText = (text: string, maxLength: number = 50) => {
        return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
    };

    return (
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 col-span-full">
            <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                    <div className="p-3 bg-purple-100 text-purple-600 rounded-lg">
                        <FaUsers size={24} />
                    </div>
                    <div>
                        <h2 className="text-lg font-semibold text-gray-900">Kullanıcı Takipleri</h2>
                        <p className="text-sm text-gray-500">Tüm takip kayıtlarını yönetin</p>
                    </div>
                </div>
                <span className="text-sm text-gray-500">Toplam: {follows.length}</span>
            </div>

            {loading ? (
                <div className="flex items-center justify-center py-8">
                    <FaSpinner className="animate-spin text-gray-400" size={24} />
                </div>
            ) : error ? (
                <div className="p-4 bg-red-50 text-red-700 rounded-lg text-sm">{error}</div>
            ) : follows.length === 0 ? (
                <div className="p-4 bg-gray-50 text-gray-500 rounded-lg text-sm text-center">
                    Henüz takip kaydı yok.
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="border-b border-gray-200">
                                <th className="text-left py-3 px-2 font-medium text-gray-600">ID</th>
                                <th className="text-left py-3 px-2 font-medium text-gray-600">Etkinlik/Sanatçı</th>
                                <th className="text-left py-3 px-2 font-medium text-gray-600">Tarih</th>
                                <th className="text-right py-3 px-2 font-medium text-gray-600">İşlem</th>
                            </tr>
                        </thead>
                        <tbody>
                            {follows.map((follow) => (
                                <tr key={follow.id} className="border-b border-gray-100 hover:bg-gray-50">
                                    <td className="py-3 px-2 text-gray-900">{follow.id}</td>
                                    <td className="py-3 px-2 text-gray-900">{truncateText(follow.artistId, 50)}</td>
                                    <td className="py-3 px-2 text-gray-500">{formatDate(follow.createdAt)}</td>
                                    <td className="py-3 px-2 text-right">
                                        <button
                                            onClick={() => handleDelete(follow.id)}
                                            disabled={deletingId === follow.id}
                                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"
                                            title="Takibi Sil"
                                        >
                                            {deletingId === follow.id ? (
                                                <FaSpinner className="animate-spin" size={16} />
                                            ) : (
                                                <FaTrash size={16} />
                                            )}
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
