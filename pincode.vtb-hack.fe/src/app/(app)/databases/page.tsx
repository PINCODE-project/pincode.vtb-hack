"use client";

import React from "react";
import { Button } from "@pin-code/ui-kit";
import { Plus } from "lucide-react";
import { useCreateDatabaseModalStore } from "@store/database/create-modal";
import Image from "next/image";
import { useTheme } from "next-themes";

export default function DatabasesPage() {
	const { theme } = useTheme();
	const { open } = useCreateDatabaseModalStore();

	return (
		<div className="p-6 space-y-6 min-h-[100svh] flex flex-col">
			<div className="mb-6 flex justify-between items-start">
				<div>
					<h1 className="text-3xl font-bold">Базы данных</h1>
					<p className="text-muted-foreground mt-2">
						Подключите вашу БД для получения анализ и рекомендации по настройке кластера
					</p>
				</div>
			</div>

			<div className="flex flex-col items-center justify-between grow">
				{/*<div className="max-w-2xl mx-auto justify-center space-y-8">*/}

				<div className="relative">
					<Image
						src={theme === "dark" ? "/images/database.png" : "/images/database-light.png"}
						alt={""}
						width={600}
						height={600}
					/>

					<Button
						onClick={open}
						size="lg"
						className="absolute bottom-10 left-[50%] translate-x-[-50%] bg-[#dd8a00] hover:bg-[#b57102] text-white px-8 py-3 rounded-xl shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-102 cursor-pointer"
					>
						<Plus className="w-5 h-5 mr-2" />
						Подключить базу данных
					</Button>
				</div>

				<div className="border-t border-border/40">
					<p className="text-sm text-muted-foreground pt-5">
						Поддерживаются PostgreSQL версии 15+, с установленным pg_stat_statements.
					</p>
				</div>
			</div>
		</div>
	);
}
