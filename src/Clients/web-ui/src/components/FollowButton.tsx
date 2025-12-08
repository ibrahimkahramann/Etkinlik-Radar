"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";

interface FollowButtonProps {
    artistId: string;
    initialIsFollowing: boolean;
}

export default function FollowButton({ artistId, initialIsFollowing }: FollowButtonProps) {
    const { data: session } = useSession();
    const [isFollowing, setIsFollowing] = useState(initialIsFollowing);
    const [isLoading, setIsLoading] = useState(false);
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    if (!mounted) {
        return (
            <button
                disabled
                className="mt-2 w-full py-2 rounded font-medium bg-gray-200 text-gray-400 cursor-not-allowed"
            >
                Yükleniyor...
            </button>
        );
    }

    const handleFollow = async () => {
        console.log("Follow button clicked for:", artistId);
        console.log("Access Token:", (session as any)?.accessToken); // Debug log
        if (!session) {
            console.log("No session found");
            alert("Takip etmek için giriş yapmalısınız.");
            return;
        }

        if (isFollowing) {
            console.log("Already following");
            // Unfollow not supported yet
            return;
        }

        setIsLoading(true);
        try {
            const res = await fetch("/api/followers", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${(session as any).accessToken}`,
                },
                body: JSON.stringify({ artistId }),
            });

            if (res.ok) {
                setIsFollowing(true);
            } else {
                const error = await res.text();
                alert(`Hata: ${error}`);
            }
        } catch (err) {
            console.error(err);
            alert("Bir hata oluştu.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <button
            onClick={handleFollow}
            disabled={isLoading || isFollowing}
            className={`mt-2 w-full py-2 rounded font-medium transition ${isFollowing
                ? "bg-green-100 text-green-700 cursor-default"
                : "bg-indigo-600 text-white hover:bg-indigo-700"
                }`}
        >
            {isLoading ? "İşleniyor..." : isFollowing ? "Takip Ediliyor" : "Takip Et"}
        </button>
    );
}
